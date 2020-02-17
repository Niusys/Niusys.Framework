using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.MessageQueue.RabbitMq
{
    /// <summary>
    /// RabbitMQ用于发送消息的连接管理池
    /// </summary>
    public abstract class PublishPool
    {
        protected string ClientName { get; set; }
        protected ConnectionSetting PublishPoolSetting { get; }
        private ILogger Logger { get; set; }
        protected IConnection CurrentConnection { get; set; }

        private const int _maxChannelQty = 30;
        private readonly Semaphore _channelSemaphore = new Semaphore(_maxChannelQty, _maxChannelQty);

        private readonly ConcurrentQueue<ChannelItem> _idleChannelQueue = new ConcurrentQueue<ChannelItem>();

        #region Write Message
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly ConcurrentQueue<Tuple<byte[], IBasicProperties, string>> _unsentMessages = new ConcurrentQueue<Tuple<byte[], IBasicProperties, string>>();
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private CancellationToken ApplicationStopingToken => _hostApplicationLifetime.ApplicationStopping;
        private readonly CancellationTokenSource _publishPoolCancelTokenSource = new CancellationTokenSource();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="connectionSetting"></param>
        /// <param name="logger"></param>
        protected PublishPool(string clientName, ConnectionSetting connectionSetting,
            ILogger logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            ClientName = clientName;
            PublishPoolSetting = connectionSetting;
            Logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            Task.Factory.StartNew(StartChannelAsync);
            Task.Factory.StartNew(FlushData);
            Task.Factory.StartNew(CleanUnsentData);
            Task.Factory.StartNew(ClearIdleChannel);

            ApplicationStopingToken.Register(() =>
            {
                WaitAllMessageSendOutAndCloseChannel();
                _publishPoolCancelTokenSource.Cancel();
            });
        }

        public virtual void Write(string messageContent, string customRoutingKey = "", string messageId = "")
        {
            if (ApplicationStopingToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("PublishPool已收到程序停止通知，不允许再将消息写入发送队列", ApplicationStopingToken);
            }

            var basicProperties = GetBasicProperties(messageId);
            var uncompressedMessage = MessageHelper.GetMessage(messageContent, _encoding);
            var message = MessageHelper.CompressMessage(uncompressedMessage, PublishPoolSetting.Compression);
            var routingKey = string.IsNullOrWhiteSpace(customRoutingKey) ? PublishPoolSetting.RoutingKey : customRoutingKey;

            AddUnsent(routingKey, basicProperties, message);
        }

        private void AddUnsent(string routingKey, IBasicProperties basicProperties, byte[] message)
        {
            _unsentMessages.Enqueue(Tuple.Create(message, basicProperties, routingKey));
        }

        private IBasicProperties GetBasicProperties(string messageId)
        {
            return new BasicProperties
            {
                ContentEncoding = "utf8",
                ContentType = PublishPoolSetting.UseJSON ? "application/json" : "text/plain",
                AppId = PublishPoolSetting.AppId,
                Timestamp = new AmqpTimestamp(DateTime.Now.GetEpochMilliseconds()),
                UserId = PublishPoolSetting.UserName, // support Validated User-ID (see http://www.rabbitmq.com/extensions.html)
                DeliveryMode = (byte)DeliveryMode.Persistent,
                MessageId = messageId
            };
        }

        #endregion

        #region Clear Idel Channel

        /// <summary>
        /// 每隔15秒清理一次没有使用的Connection
        /// </summary>
        private void ClearIdleChannel()
        {
            while (true)
            {
                if (_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    break;
                }
                _channelSemaphore.WaitOne();
                try
                {
                    if (_idleChannelQueue.TryDequeue(out var channelItem))
                    {
                        if (channelItem.Channel != null && channelItem.Channel.IsClosed)
                        {
                            channelItem.Channel.Close();
                            channelItem.Channel.Dispose();
                        }
                        else if (channelItem.Channel != null && channelItem.Channel.IsOpen)
                        {
                            //Channel is open
                            if ((DateTime.Now - channelItem.LastUseTime) > TimeSpan.FromSeconds(15))
                            {
                                SafeCloseRabbitMqChannel(channelItem, "IdleClear");
                            }
                            else
                            {
                                _idleChannelQueue.Enqueue(channelItem);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                _channelSemaphore.Release();

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        private static void SafeCloseRabbitMqChannel(ChannelItem channelItem, string reason)
        {
            if (channelItem.Channel.IsOpen)
            {
                channelItem.Channel.Abort(Constants.NoConsumers, reason);
                channelItem.Channel.Close(Constants.NoConsumers, reason);
            }
            channelItem.Channel.Dispose();
        }

        #endregion

        /// <summary>
        /// 异步方式启动Channel,
        /// </summary>
        /// <returns></returns>
        private async Task StartChannelAsync()
        {
            //1. Connection在初始连接时出现问题, Policy会不断的重试.
            //2. Connection在初始连接之后，后续因为网络不稳，不断的掉线, 在网络恢复时Connection会自动恢复连接, 这个由RabbitMQ的驱动完成
            var rabbitmqConnectRetryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetryForeverAsync((retryAttempt) =>
                {
                    return TimeSpan.FromSeconds(5);
                }, (exception, retryAttempt, timespan) =>
                {
                    Logger.LogError($"连接RabbitMq[{PublishPoolSetting.HostName}{PublishPoolSetting.VHost}]失败, {timespan.TotalSeconds}秒后进行第{retryAttempt}次重试");
                });

            await rabbitmqConnectRetryPolicy.ExecuteAsync(async cancellationToke =>
            {
                Logger.LogWarning("尝试连接RabbitMQ");
                CurrentConnection = GetConnectionFac().CreateConnection($"miaoapi-{ClientName}_ClientPool-{Environment.MachineName}");

                CurrentConnection.RecoverySucceeded += (s, e) =>
                {
                    Logger.LogWarning($"RabbitMQ连接已恢复");
                };
                CurrentConnection.ConnectionRecoveryError += (s, e) =>
                {
                    Logger.LogWarning($"RabbitMQ连接尝试恢复错误, {e.Exception.FullMessage()}");
                };
                CurrentConnection.ConnectionShutdown += (s, e) =>
                {
                    Logger.LogWarning($"RabbitMQ连接已断开, {e.ReplyCode} {e.ReplyText}");
                };
                Logger.LogWarning("RabbitMQ连接成功");
                await Task.CompletedTask.ConfigureAwait(false);
            }, _hostApplicationLifetime.ApplicationStopping).ConfigureAwait(false);
        }

        private ConnectionFactory GetConnectionFac()
        {
            return new ConnectionFactory
            {
                HostName = PublishPoolSetting.HostName,
                VirtualHost = PublishPoolSetting.VHost,
                UserName = PublishPoolSetting.UserName,
                Password = PublishPoolSetting.Password,
                RequestedHeartbeat = PublishPoolSetting.HeartBeatSeconds,
                Port = PublishPoolSetting.Port,
                Ssl = new SslOption()
                {
                    Enabled = PublishPoolSetting.UseSsl,
                    CertPath = PublishPoolSetting.SslCertPath,
                    CertPassphrase = PublishPoolSetting.SslCertPassphrase,
                    ServerName = PublishPoolSetting.HostName
                }
            };
        }

        #region Flush Message to ReabbitMQ
        /// <summary>
        /// FlushData
        /// </summary>
        /// <returns></returns>
        private async Task FlushData()
        {
            DateTime lastNotifyTime = DateTime.Now.AddMinutes(-1);
            var sleepTimespan = TimeSpan.FromMilliseconds(200);
            while (true)
            {
                try
                {
                    if (_unsentMessages.Count > 0)
                    {
                        var isDisconnected = !CurrentConnection?.IsOpen ?? true;

                        if (isDisconnected && (lastNotifyTime - DateTime.Now).TotalSeconds >= 30)
                        {
                            lastNotifyTime = DateTime.Now;
                            Logger.LogWarning($"RabbitMq 连接状态为 未开启");
                        }
                    }
                    else
                    {
                        //当unsetMessage为空时,检查是否停止
                        if (_publishPoolCancelTokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    // using a queue so that removing and publishing is a single operation
                    while (_unsentMessages.Count > 0 && CurrentConnection != null && CurrentConnection.IsOpen)
                    {
                        if (!_unsentMessages.TryDequeue(out var tuple))
                        {
                            continue;
                        }
                        bool isSendSuccess = PubMessageInternal(model =>
                        {
                            model.BasicPublish(PublishPoolSetting.Exchange, tuple.Item3, true, tuple.Item2, tuple.Item1);
                        });

                        if (!isSendSuccess)
                        {
                            _unsentMessages.Enqueue(tuple);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"MqPublishPool FlushData Exception: {Environment.NewLine}{ex.FullMessage()}{Environment.NewLine}{ex.FullStacktrace()}");
                }

                await Task.CompletedTask;
                Thread.Sleep(sleepTimespan);//休眠1秒之后继续尝试连接
            }
        }

        private bool PubMessageInternal(Action<IModel> action)
        {
            var sendResult = false;
            try
            {
                _channelSemaphore.WaitOne();
                ChannelItem channelItem;
                do
                {
                    if (_idleChannelQueue.TryDequeue(out channelItem))
                    {
                        continue;
                    }
                    //再次尝试从idleChannel中获取, 获取失败意味着没有空闲的Channel, 则重新创建一个Channel
                    var model = CurrentConnection?.CreateModel();
                    if (model == null)
                    {
                        Logger.LogError($"创建RabbitMQ Channel失败, 原因{nameof(CurrentConnection)}为null");
                    }
                    else
                    {
                        //创建Channel成功, 声明Exchange并开始处理数据
                        model.ExchangeDeclare(PublishPoolSetting.Exchange, PublishPoolSetting.ExchangeType, PublishPoolSetting.Durable);
                        _idleChannelQueue.Enqueue(new ChannelItem(model));
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));//休眠1秒之后继续尝试连接
                } while (channelItem == null || !EnsureChannelHealthy(channelItem.Channel));

                action(channelItem.Channel);

                channelItem.LastUseTime = DateTime.Now;
                _idleChannelQueue.Enqueue(channelItem);
                _channelSemaphore.Release();
                sendResult = true;
            }
            catch (IOException e)
            {
                Logger.LogError($"MQ消息发送IOException {e.FullMessage()}");
            }
            catch (ObjectDisposedException e)
            {
                Logger.LogError($"MQ消息发送ObjectDisposedException {e.FullMessage()}");
            }
            catch (AlreadyClosedException e)
            {
                Logger.LogError($"MQ消息发送失败:{e.FullMessage()}");
            }
            catch (Exception e)
            {
                Logger.LogError($"MQ消息发送失败, ExType: {e.GetType().FullName}, Message:{e.FullMessage()}, {e.FullStacktrace()}");
            }
            return sendResult;
        }

        private bool EnsureChannelHealthy(IModel channel)
        {
            return channel != null && !channel.IsClosed;
        }

        #endregion

        public void WaitAllMessageSendOutAndCloseChannel()
        {
            int waitMs = 500;
            Logger.LogInformation("WaitAllMessageSendOutAndCloseChannel");

            while (true)
            {
                if (_unsentMessages.Count > 0)
                {
                    Logger.LogInformation($"UnsendMessage队列剩余{_unsentMessages.Count}条消息等待发送, 等待{waitMs}ms之后再检查");
                    Task.Delay(waitMs);
                }
                else
                {
                    break;
                }
            }

            Logger.LogInformation("开始关闭Channel");
            while (_idleChannelQueue.TryDequeue(out var channelItem))
            {
                SafeCloseRabbitMqChannel(channelItem, "Application Stoping");
            }
        }


        private void CleanUnsentData()
        {
            while (true)
            {
                if (ApplicationStopingToken.IsCancellationRequested)
                {
                    break;
                }

                for (var i = 0; i < _unsentMessages.Count - PublishPoolSetting.MaxBuffer; i++)
                {
                    _unsentMessages.TryDequeue(out var _);
                }
                Thread.Sleep(5000);
            }
        }

        private class ChannelItem
        {
            public IModel Channel { get; }
            public DateTime LastUseTime { get; set; }

            public ChannelItem(IModel channel)
            {
                Channel = channel;
                LastUseTime = DateTime.Now;
            }
        }
    }
}
