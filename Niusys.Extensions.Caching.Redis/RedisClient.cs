using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Niusys.Extensions.Caching.Redis
{
    public class RedisClient : IRedisClient
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisClient> _logger;
        private readonly AsyncRetryPolicy _redisOperationPolicy;

        public RedisClient(IDatabase database, ILoggerFactory loggerFactory)
        {
            this._database = database;
            _logger = loggerFactory.CreateLogger<RedisClient>();
            _redisOperationPolicy = Policy
                .Handle<RedisTimeoutException>()
                .WaitAndRetryAsync(2, (retryAttempt) =>
                {
                    return TimeSpan.FromSeconds(2);
                });
        }

        public Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashDeleteAsync(key, hashField, flags);
        }

        public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashExists(key, hashField, flags);
        }

        public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashExistsAsync(key, hashField, flags);
        }

        public RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashGet(key, hashField, flags);
        }

        public Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashGetAllAsync(key, flags);
        }

        public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashGetAsync(key, hashField, flags);
        }

        public bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashSet(key, hashField, value, when, flags);
        }

        public Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashSetAsync(key, hashField, value, when, flags);
        }

        public Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            return _database.HashSetAsync(key, hashFields, flags);
        }

        public Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.KeyDeleteAsync(key, flags));
        }

        public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.KeyExistsAsync(key, flags));
        }

        public bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            return _database.KeyExpire(key, expiry, flags);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.KeyExpireAsync(key, expiry, flags));
        }

        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.KeyExpireAsync(key, expiry, flags));
        }

        public RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            return _database.ListRange(key, start, stop, flags);
        }

        public long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return _database.ListRightPush(key, value, when, flags);
        }

        public long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return _database.ListRightPush(key, values, flags);
        }

        public Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return _database.SetAddAsync(key, value, flags);
        }

        public Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return _database.SetAddAsync(key, values, flags);
        }

        public Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return _database.SetContainsAsync(key, value, flags);
        }

        public Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return _database.SetLengthAsync(key, flags);
        }

        public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return _database.SortedSetAddAsync(key, values, when, flags);
        }

        public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags)
        {
            return _database.SortedSetAddAsync(key, member, score, flags);
        }

        public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return _database.SortedSetAddAsync(key, member, score, when, flags);
        }

        public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags)
        {
            return _database.SortedSetAddAsync(key, values, flags);
        }

        public Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return _database.SortedSetLengthAsync(key, min, max, exclude, flags);
        }

        public Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return _database.SortedSetRangeByRankAsync(key, start, stop, order, flags);
        }

        public Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return _database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, flags);
        }

        public Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            return _database.SortedSetRemoveRangeByRankAsync(key, start, stop, flags);
        }

        public Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.StringDecrementAsync(key, value, flags));
        }

        public Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.StringDecrementAsync(key, value, flags));
        }

        public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.StringGetAsync(key, flags));
        }

        public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.StringIncrementAsync(key, value, flags));
        }

        public Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.StringIncrementAsync(key, value, flags));
        }

        public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.StringSetAsync(key, value, expiry, when, flags));
        }

        public Task<RedisResult> ExecuteAsync(string command, params object[] args)
        {
            return _database.ExecuteAsync(command, args);
        }
        public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            return _database.ScriptEvaluateAsync(script, keys, values, flags);
        }

        public Task<RedisResult> ScriptEvaluateAsync(LuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
        {
            return _database.ScriptEvaluateAsync(script, parameters, flags);
        }

        public Task<RedisResult> ScriptEvaluateAsync(LoadedLuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
        {
            return _database.ScriptEvaluateAsync(script, parameters, flags);
        }

        #region 自定义方法

        /// <summary>
        ///
        /// </summary>
        /// <param name="keyPattern"></param>
        /// <returns></returns>
        public async Task SafeClearRedisKeyPattern(string keyPattern)
        {
            try
            {
                List<RedisKey> keys = new List<RedisKey>();
                foreach (System.Net.EndPoint endpoint in _database.Multiplexer.GetEndPoints())
                {
                    IServer server = _database.Multiplexer.GetServer(endpoint);
                    //忽略Slave节点上的值
                    if (!server.IsSlave)
                    {
                        //Database always use first database's index
                        IEnumerable<RedisKey> dbKeys = server.Keys(_database.Database, keyPattern);
                        keys.AddRange(dbKeys);
                    }
                }

                foreach (var item in keys)
                {
                    await _database.KeyDeleteAsync(item);
                }
                //await database.ScriptEvaluateAsync("redis.call('del', unpack(redis.call('keys',ARGV[1])))", values: new RedisValue[] { keyPattern });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.FullMessage());
            }
        }
        #endregion

        public Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.LockTakeAsync(key, value, expiry, flags));
        }

        public Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return _redisOperationPolicy.ExecuteAsync(() => _database.LockReleaseAsync(key, value, flags));
        }
    }
}
