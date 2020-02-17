using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Caching.Redis
{
    /// <summary>
    /// Redis Store
    /// </summary>
    public class RedisStore : IRedisStore
    {
        private volatile ConnectionMultiplexer _connection;
        private volatile ConnectionMultiplexer _adminConnection;
        private readonly RedisSettings _redisSettings;
        private readonly Semaphore _connectionLock = new Semaphore(initialCount: 1, maximumCount: 1);
        private readonly ILogger<RedisStore> _logger;

        public RedisStore(IOptions<RedisSettings> dbSettings, ILogger<RedisStore> logger)
        {
            _redisSettings = dbSettings.Value;
            this._logger = logger;
        }

        private ConfigurationOptions GetConfigurationOptions(bool allowAdmin = false)
        {
            var redisEndpoint = _redisSettings.Host;
            if (string.IsNullOrWhiteSpace(redisEndpoint))
            {
                throw new Exception("没有找到RedisServer:Host的配置");
            }

            var configurationOptions = new ConfigurationOptions
            {
                Password = _redisSettings.Password,
                AbortOnConnectFail = true,
                Ssl = false,
                AllowAdmin = allowAdmin,
                //连接超时不可以设置的过短,会导致很多初始化失败,建立连接本身是一个比较耗时的操作
                ConnectTimeout = 10000
            };

            foreach (var item in redisEndpoint.Split(','))
            {
                configurationOptions.EndPoints.Add(item);
            }

            return configurationOptions;
        }

        private void Connect()
        {
            if (_connection != null)
            {
                if (_connection.IsConnected)
                {
                    return;
                }
                else
                {
                    throw new Exception("Redis连接已断开,等待自动恢复");
                }
            }

            if (_connectionLock.WaitOne(TimeSpan.FromSeconds(2)))
            {
                try
                {
                    if (_connection != null)
                    {
                        if (_connection.IsConnected)
                        {
                            return;
                        }
                        else
                        {
                            throw new Exception("Redis连接已断开,等待自动恢复");
                        }
                    }

                    var options = GetConfigurationOptions();

                    var redisConnectRetryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryForever((retryAttempt) =>
                        {
                            return TimeSpan.FromSeconds(2);
                        }, (exception, retryAttempt, timespan) =>
                        {
                            Console.WriteLine($@"{nameof(RedisStore)}连接Redis服务器{string.Join(",", options.EndPoints.Select(x => x.ToString()))}失败,
ExType:{exception.GetType().FullName} Cause:{exception.Message}, {timespan.TotalSeconds}秒后进行第{retryAttempt}次重试");
                        });

                    #region Redis连接服务器
                    redisConnectRetryPolicy.Execute(() =>
                    {
                        Console.WriteLine("start connect to redis server");
                        _connection = ConnectionMultiplexer.Connect(options);
                        _connection.ConnectionFailed += (s, e) =>
                        {
                            Console.WriteLine($"Redis ConnectionFaild, {e.Exception?.Message}");
                        };
                        _connection.ConnectionRestored += (s, e) =>
                        {
                            Console.WriteLine($"Redis ConnectionRestored, {e.Exception?.Message}");
                        };
                        _connection.ErrorMessage += (s, e) =>
                        {
                            Console.WriteLine($"Redis ErrorMessage, {e.Message}");
                        };
                        _connection.InternalError += (s, e) =>
                        {
                            Console.WriteLine($"Redis InternalError, {e.Exception?.Message}");
                        };
                        Console.WriteLine("redis server connected");
                    });
                    #endregion
                }
                finally
                {
                    _connectionLock.Release();
                }
            }
            else
            {
                throw new Exception("Redis正在连接中, 获取当前连接失败");
            }
        }

        private void ConnectAdmin(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (_adminConnection != null && _adminConnection.IsConnected)
            {
                return;
            }

            _connectionLock.WaitOne();
            try
            {
                if (_adminConnection != null && _adminConnection.IsConnected)
                {
                    return;
                }

                var options = GetConfigurationOptions(true);
                try
                {
                    _adminConnection = ConnectionMultiplexer.Connect(options);
                }
                catch (RedisConnectionException ex)
                {
                    throw new Exception($"Redis服务器连接失败, 连接地址:{string.Join(",", options.EndPoints.Select(x => x.ToString()))}, 错误信息:{ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Redis服务器连接失败, 连接地址:{string.Join(",", options.EndPoints.Select(x => x.ToString()))}, 错误信息:{ex.Message}", ex);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private static string TryResolveDns(string redisUrl)
        {
            var isIp = IsIpAddress(redisUrl);
            if (isIp)
            {
                return redisUrl;
            }
            var ip = Dns.GetHostEntryAsync(redisUrl).GetAwaiter().GetResult();
            return $"{ip.AddressList.First(x => IsIpAddress(x.ToString()))}";
        }

        private static bool IsIpAddress(string host)
        {
            return Regex.IsMatch(host, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        }

        public IDatabase GetDatabaseAsync(int databaseIndex = 0, bool alwaysNewDatabase = false, CancellationToken token = default)
        {
            Connect();
            return _connection.GetDatabase(databaseIndex);
        }

        public IConnectionMultiplexer GetConnectionMultiplexer(bool adminEnable = false, CancellationToken token = default)
        {
            if (adminEnable)
            {
                ConnectAdmin(token);
                return _adminConnection;
            }
            else
            {
                Connect();
                return _connection;
            }
        }

        Task<IConnectionMultiplexer> IRedisStore.GetConnectionMultiplexer(bool allowAdmin, CancellationToken token)
        {
            return Task.FromResult(GetConnectionMultiplexer(allowAdmin, token));
        }

        Task<IDatabase> IRedisStore.GetDatabaseAsync(int databaseIndex, bool alwaysNewDatabase, CancellationToken token)
        {
            return Task.FromResult(GetDatabaseAsync(databaseIndex, alwaysNewDatabase, token));
        }
    }
}
