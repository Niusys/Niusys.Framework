using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Niusys.Extensions.MessageQueue.RabbitMq;
using Niusys.Extensions.Storage.Mongo;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Buses
{
    /// <summary>
    /// MIAO系统所有消息消费端的基类，处理基本的消息接收与响应
    /// </summary>
    public abstract class MessageConsumer : IMessageConsumer
    {
        private ILogger _logger;
        private const int _retryTimeSpanBaseSeconds = 2;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionSetting"></param>
        /// <param name="logger"></param>
        /// <param name="prefetchCount"></param>
        protected MessageConsumer(ConnectionSetting connectionSetting,
            ILogger logger,
            ushort prefetchCount = 1000)
        {
            ConnectionSetting = connectionSetting;
            _logger = logger;
            PrefetchCount = prefetchCount;
        }

        public ushort PrefetchCount { get; }
        protected virtual bool MessageDurable { get; } = true;
        protected virtual bool IsConsumerTraceEnabled { get; } = true;
        protected virtual bool MoveToErrorQueueWhenErrorOccured { get; set; } = true;
        public IHostEnvironment HostingEnvironment { get; set; }

        /// <summary>
        /// 消息消费时出现错误是否可以重试, 默认不可以
        /// </summary>
        protected virtual bool MessageCanRetryWhenErrorOccured { get; set; } = false;

        /// <summary>
        /// 启动Consumer
        /// </summary>
        /// <param name="sessionName"></param>
        public void StartConsume(string sessionName)
        {
            SessionName = sessionName;
            /*
             Start Connection, Per Consumer Per Connection Mulit Channel Policy
             */
            StartChannel().Wait(CancellationToken);

            Task.Factory.StartNew(FlushErrorData);

            HostingEnvironment = ServiceProvider.GetService<IHostEnvironment>();
        }

        /// <summary>
        /// 异步方式启动Channel,
        /// todo: 1.需要考虑Connection出现问题如何处理, 2. Connection不稳定会怎么样
        /// </summary>
        /// <returns></returns>
        private async Task StartChannel()
        {
            var rabbitmqConnectRetryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetryForeverAsync((retryAttempt) =>
                {
                    return TimeSpan.FromSeconds(5);
                }, (exception, retryAttempt, timespan) =>
                {
                    _logger.LogError($"连接RabbitMq[{ConnectionSetting.HostName}{ConnectionSetting.VHost}]失败, {timespan.TotalSeconds}秒后进行第{retryAttempt}次重试");
                });

            await rabbitmqConnectRetryPolicy.ExecuteAsync(async cancellationToke =>
            {
                _logger.LogWarning("尝试连接RabbitMQ");
                CurrentConnection = GetConnectionFac().CreateConnection($"miaoapi-{SessionName}_ClientPool-{Environment.MachineName}");

                CurrentConnection.RecoverySucceeded += (s, e) =>
                {
                    _logger.LogWarning($"RabbitMQ连接已恢复");
                };
                CurrentConnection.ConnectionRecoveryError += (s, e) =>
                {
                    _logger.LogWarning($"RabbitMQ连接尝试恢复错误, {e.Exception.FullMessage()}");
                };
                CurrentConnection.ConnectionShutdown += (s, e) =>
                {
                    _logger.LogWarning($"RabbitMQ连接已断开, {e.ReplyCode} {e.ReplyText}");
                };
                _logger.LogWarning("RabbitMQ连接成功");

                var CurrentConsumerChannel = CurrentConnection.CreateModel();
                BindCurrentQueueToExchange(CurrentConsumerChannel);

                var currentConsumer = new EventingBasicConsumer(CurrentConsumerChannel);
                currentConsumer.Received += ConsumerReceiveHandler;
                currentConsumer.Shutdown += (s, e) => { _logger.LogWarning($"RabbitMQ Consumer Channel Shutdown Event"); };
                currentConsumer.ConsumerCancelled += (s, e) => { _logger.LogWarning($"RabbitMQ Consumer Channel ConsumerCancelled Event"); };
                currentConsumer.Registered += (s, e) => { _logger.LogWarning($"RabbitMQ Consumer Channel Registered Event"); };
                currentConsumer.Unregistered += (s, e) => { _logger.LogWarning($"RabbitMQ Consumer Channel Unregistered Event "); };
                CurrentConsumerChannel.BasicQos(0, PrefetchCount, false);
                CurrentConsumerChannel.BasicConsume(queue: QueueName, autoAck: false, consumer: currentConsumer);

                await Task.CompletedTask;
            }, CancellationToken);
        }

        private void BindCurrentQueueToExchange(IModel channel)
        {
            if (ConnectionSetting.Exchange.IsNullOrWhitespace())
            {
                return;
            }

            channel.ExchangeDeclare(ConnectionSetting.Exchange, "topic", true);
            if (QueueName.IsNullOrWhitespace())
            {
                return;
            }

            channel.QueueDeclare(QueueName, MessageDurable, false, false, null);
            foreach (var item in BindRoutingKeyPattern.Split('|'))
            {
                channel.QueueBind(QueueName, ConnectionSetting.Exchange, item);
            }
        }

        #region 消息处理循环

        /// <summary>
        /// 消息消费处理方法
        /// </summary>
        /// <param name="e"></param>
        /// <param name="bodyMessage"></param>
        /// <param name="serviceProvider"></param>
        protected abstract Task ConsumerAsync(BasicDeliverEventArgs e, string bodyMessage, IServiceProvider serviceProvider);


        private const string _lDS_NAME = "consumer";
        /// <summary>
        /// 消息的接收事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private void ConsumerReceiveHandler(object sender, BasicDeliverEventArgs ea)
        {
            StopwatchAction((stopwatch) =>
            {
                //If the channel is closed, directly return no need to handle;
                var currentConsumerLoopChannel = (sender as IBasicConsumer)?.Model;
                if (currentConsumerLoopChannel == null || currentConsumerLoopChannel.IsClosed)
                {
                    return;
                }

                if (CancellationToken.IsCancellationRequested)
                {
                    StopConsume();//释放本地对象
                    return;
                }

                ConsumeHandleResult consumeHandleResult = ConsumeHandleResult.Act;
                try
                {
                    var bodyMessage = Encoding.UTF8.GetString(ea.Body);
                    var cts = new CancellationTokenSource(HostingEnvironment?.IsDevelopment() ?? false ? TimeSpan.FromMinutes(30) : TimeSpan.FromMinutes(30));
                    using (var serviceScope = ServiceProvider.CreateScope())
                    {
                        ConsumerAsync(ea, bodyMessage, serviceScope.ServiceProvider).Wait(cts.Token);
                    }
                    consumeHandleResult = ConsumeHandleResult.Act;
                    AcknowledgeMessage(ea, currentConsumerLoopChannel, consumeHandleResult);
                }
                catch (AggregateException ex)
                {
                    _logger.LogError(ex, $"消息消费时出现异常,将推送到消费错误队列进行人工处理, 详细错误:{ex.FullMessage()}");
                    consumeHandleResult = ConsumeHandleResult.ActAndMoveToErrorQueue;
                    var storedException = ex.InnerExceptions.Select(x => new { Message = x.FullMessage(), StackTrace = x.FullStacktrace(), ExType = x.FullExType() }).ToList();
                    AcknowledgeMessage(ea, currentConsumerLoopChannel, consumeHandleResult, new { Errors = storedException });
                }
                catch (TaskCanceledException ex)
                {
                    consumeHandleResult = ConsumeHandleResult.ActAndMoveToErrorQueue;
                    _logger.LogError(ex, $"消息消费出现[TaskCanceledException]异常,并将推送到消费错误队列进行人工处理, 详细错误:{ex.FullMessage()}");
                    var storedException = new { Message = ex.FullMessage(), StackTrace = ex.FullStacktrace(), ExType = ex.FullExType() };
                    AcknowledgeMessage(ea, currentConsumerLoopChannel, consumeHandleResult, storedException);
                }
                catch (OperationCanceledException ex)
                {
                    consumeHandleResult = ConsumeHandleResult.ActAndMoveToErrorQueue;
                    _logger.LogError(ex, $"消息消费超时OperationCanceledException(超时时间120秒), 并将推送到消费错误队列进行人工核查处理, 详细错误:{ex.FullMessage()}");
                    var storedException = new { Message = ex.FullMessage(), StackTrace = ex.FullStacktrace(), ExType = ex.FullExType() };
                    AcknowledgeMessage(ea, currentConsumerLoopChannel, consumeHandleResult, storedException);
                }
                catch (Exception ex)
                {
                    consumeHandleResult = ConsumeHandleResult.ActAndMoveToErrorQueue;
                    _logger.LogError(ex, $"消息消费出现异常[{ex.GetType().FullName}],并将推送到消费错误队列进行人工处理, 详细错误:{ex.FullMessage()}");
                    var storedException = new { Message = ex.FullMessage(), StackTrace = ex.FullStacktrace(), ExType = ex.FullExType() };
                    AcknowledgeMessage(ea, currentConsumerLoopChannel, consumeHandleResult, storedException);
                    //only no particular exception have the retry
                }

            }, SessionName + " consumer message used:{0} ms", sw =>
            {
                //todo: 耗时分析
            });
        }


        /// <summary>
        /// 调用的地方必须用using做释放
        /// </summary>
        /// <returns></returns>
        protected IServiceScope CreateServiceScope()
        {
            return ServiceProvider.CreateScope();
        }

        /// <summary>
        /// 通知Mq消息处理结果
        /// </summary>
        /// <param name="ea"></param>
        /// <param name="currentConsumerLoopChannel"></param>
        /// <param name="consumeHandleResult"></param>
        /// <param name="storedException"></param>
        private void AcknowledgeMessage(BasicDeliverEventArgs ea, IModel currentConsumerLoopChannel, ConsumeHandleResult consumeHandleResult, object storedException = null)
        {
            try
            {
                switch (consumeHandleResult)
                {
                    case ConsumeHandleResult.Act:
                        currentConsumerLoopChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        //重试消息处理逻辑
                        if (ea.BasicProperties.MessageId.IsNotNullOrWhitespace())
                        {
                            var messageId = ea.BasicProperties.MessageId;
                            var consumerErrorMessageRepository = ServiceProvider.GetService<IConsumerErrorMessageStore>();
                            consumerErrorMessageRepository.DeleteAsync(messageId.SafeToObjectId()).Wait();
                        }
                        break;
                    case ConsumeHandleResult.ActAndMoveToErrorQueue:
                        currentConsumerLoopChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        //重试消息处理逻辑
                        if (ea.BasicProperties.MessageId.IsNotNullOrWhitespace())
                        {
                            var messageId = ea.BasicProperties.MessageId;
                            var consumerErrorMessageRepository = ServiceProvider.GetService<IConsumerErrorMessageStore>();
                            var message = consumerErrorMessageRepository.GetByIdAsync(messageId.SafeToObjectId()).Result;
                            message.ProcessTimes += 1;//已重试次数+1

                            var originalBodyMessage = Encoding.UTF8.GetString(ea.Body);
                            var orignalBodyMessageObject = BsonSerializer.Deserialize<BsonDocument>(originalBodyMessage);
                            var update = Builders<ConsumerErrorMessage>.Update
                                 .Set(x => x.ProcessTimes, message.ProcessTimes)
                                 .Set(x => x.RetryTime, DateTime.Now)
                                 .Set(x => x.RetryNow, 0)
                                 .Set(x => x.RetryLock, null)
                                 .Set(x => x.Exception, storedException.ToBsonDocument())
                                 .Set(x => x.LastedOriginalMessage, orignalBodyMessageObject)
                                 .Set(x => x.NextRetryTime, DateTime.Now.AddMinutes(Math.Pow(_retryTimeSpanBaseSeconds, message.ProcessTimes)));//计算下次重试时间

                            consumerErrorMessageRepository.UpdateAsync(Builders<ConsumerErrorMessage>.Filter.Eq(x => x.Sysid, messageId.SafeToObjectId()), update).Wait();
                        }
                        else
                        {
                            //如当前Consumer开启MoveToErrorQueue则推送到错误队列
                            if (MoveToErrorQueueWhenErrorOccured)
                            {
                                var originalBodyMessage = Encoding.UTF8.GetString(ea.Body);
                                var orignalBodyMessageObject = JObject.Parse(originalBodyMessage);

                                var errorMessageModel = new
                                {
                                    RoutingKey = ea.RoutingKey,
                                    Exchange = ea.Exchange,
                                    Logged = DateTime.Now,
                                    OrignalMessage = orignalBodyMessageObject,
                                    Exception = storedException,
                                    CanRetry = MessageCanRetryWhenErrorOccured ? 1 : 0,
                                    ProcessTimes = 1,
                                    NextRetryTime = DateTime.Now.AddMinutes(Math.Pow(_retryTimeSpanBaseSeconds, 1))//计算第二次的重试时间
                                };

                                var uncompressedMessage = MessageHelper.GetMessage(JsonConvert.SerializeObject(errorMessageModel, new JsonSerializerSettings()
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                }), Encoding.UTF8);
                                var sendMessage = MessageHelper.CompressMessage(uncompressedMessage, CompressionTypes.None);

                                RecordConsumerError(ea.RoutingKey, sendMessage);
                            }
                        }
                        break;
                    case ConsumeHandleResult.Reject:
                        currentConsumerLoopChannel.BasicReject(ea.DeliveryTag, true);//拒绝消息, 重新插入队列
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"MessageConsume消息消费发生异常之后, 在处理异常的过程中再次发生错误, EXType:{ex.GetType().FullName} 错误详细信息: {ex.FullMessage()}");
            }
        }

        private readonly ConcurrentQueue<Tuple<byte[], IBasicProperties, string>> _unsentErrorMessages = new ConcurrentQueue<Tuple<byte[], IBasicProperties, string>>();

        private void RecordConsumerError(string routingKey, byte[] sendMessage)
        {
            _unsentErrorMessages.Enqueue(new Tuple<byte[], IBasicProperties, string>(sendMessage, GetBasicProperties(), routingKey));
        }

        private async Task FlushErrorData()
        {
            DateTime lastNotifyTime = DateTime.Now.AddMinutes(-1);
            //消费错误消息,2秒检查一次
            var sleepTimespan = TimeSpan.FromMilliseconds(2000);
            while (true)
            {
                if (_unsentErrorMessages.Count > 0)
                {
                    var isDisconnected = !CurrentConnection?.IsOpen ?? true;

                    if (isDisconnected && (lastNotifyTime - DateTime.Now).TotalSeconds >= 30)
                    {
                        lastNotifyTime = DateTime.Now;
                        _logger.LogWarning($"RabbitMq 连接状态为 未开启");

                        ErrorMessageBufferCheckAndClean();
                    }
                }

                List<Tuple<byte[], IBasicProperties, string>> batchPublishBuffer = new List<Tuple<byte[], IBasicProperties, string>>();
                while (_unsentErrorMessages.Count > 0 && CurrentConnection != null && CurrentConnection.IsOpen)
                {
                    if (!_unsentErrorMessages.TryDequeue(out var tuple))
                    {
                        continue;
                    }

                    batchPublishBuffer.Add(tuple);

                    if (batchPublishBuffer.Count >= 20)
                    {
                        break;
                    }
                }

                try
                {
                    if (batchPublishBuffer.Count > 0)
                    {
                        using (var errorChannel = CurrentConnection.CreateModel())
                        {
                            var basicBashPublisher = errorChannel.CreateBasicPublishBatch();
                            foreach (var item in batchPublishBuffer)
                            {
                                basicBashPublisher.Add(Consts.ERROR_EXCHANGE, item.Item3, true, item.Item2, item.Item1);
                            }
                            basicBashPublisher.Publish();
                            batchPublishBuffer.Clear();

                            //安全关闭Channel
                            errorChannel.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"MessageConsumer FlushData Exception: ExType:{ex.FullExType()}, Msg:{Environment.NewLine}{ex.FullMessage()}{Environment.NewLine}{ex.FullStacktrace()}");
                }

                await Task.CompletedTask;
                Thread.Sleep(sleepTimespan);//休眠1秒之后继续尝试连接
            }
        }

        private void ErrorMessageBufferCheckAndClean()
        {
            //清理队列数据
            var ErrorQueueMaxBuffer = 1000;
            var prepareCleanCount = _unsentErrorMessages.Count - ErrorQueueMaxBuffer;
            if (prepareCleanCount > 0)
            {
                _logger.LogWarning($"Prepare clean {prepareCleanCount} unsend recoards");
                for (var i = 0; i < prepareCleanCount && _unsentErrorMessages.Count > ErrorQueueMaxBuffer && _unsentErrorMessages.TryDequeue(out var _); i++)
                {

                }
            }
        }

        private IBasicProperties GetBasicProperties()
        {
            return new BasicProperties
            {
                ContentEncoding = "utf8",
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTime.Now.GetEpochSeconds()),
                DeliveryMode = (byte)DeliveryMode.Persistent
            };
        }
        #endregion

        private IConnection CurrentConnection { get; set; }

        /// <summary>
        ///
        /// </summary>
        public void PrepareClose()
        {

        }

        /// <summary>
        ///
        /// </summary>
        public void StopConsume()
        {
            CurrentConnection.Close(200, "Client Exit");
        }

        /// <summary>
        ///
        /// </summary>
        public string SessionName { get; private set; }
        public abstract string QueueName { get; set; }
        public abstract string BindRoutingKeyPattern { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsStoped { get; private set; }

        /// <summary>
        ///
        /// </summary>
        protected ConnectionSetting ConnectionSetting { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        ///
        /// </summary>
        private IServiceProvider ServiceProvider { get; set; }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        #region Utils

        private ConnectionFactory GetConnectionFac()
        {
            return new ConnectionFactory
            {
                HostName = ConnectionSetting.HostName,
                VirtualHost = ConnectionSetting.VHost,
                UserName = ConnectionSetting.UserName,
                Password = ConnectionSetting.Password,
                RequestedHeartbeat = ConnectionSetting.HeartBeatSeconds,
                Port = ConnectionSetting.Port,
                Ssl = new SslOption()
                {
                    Enabled = ConnectionSetting.UseSsl,
                    CertPath = ConnectionSetting.SslCertPath,
                    CertPassphrase = ConnectionSetting.SslCertPassphrase,
                    ServerName = ConnectionSetting.HostName
                }
            };
        }

        private void StopwatchAction(Action<Stopwatch> action, string format, Action<Stopwatch> callback = null)
        {
            var sw = Stopwatch.StartNew();
            action(sw);
            sw.Stop();
            if (IsConsumerTraceEnabled)
            {
                _logger.LogDebug(format, sw.ElapsedMilliseconds);
            }
        }

        #endregion
    }
}
