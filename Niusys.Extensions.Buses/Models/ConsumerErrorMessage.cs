using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Niusys.Extensions.Storage.Mongo;
using System;

namespace Niusys.Extensions.Buses
{
    /// <summary>
    /// 消费失败的消息
    /// </summary>
    [BsonIgnoreExtraElements, MongoCollection(CollectionName = nameof(ConsumerErrorMessage))]
    public class ConsumerErrorMessage : MongoEntity
    {
        public string RoutingKey { get; set; }
        public string Exchange { get; set; }
        public DateTime Logged { get; set; }

        /// <summary>
        /// 处理次数
        /// </summary>
        public int ProcessTimes { get; set; }

        public BsonDocument OrignalMessage { get; set; }

        public BsonValue Exception { get; set; }

        /// <summary>
        /// 是否可以重试
        /// </summary>
        public int CanRetry { get; set; }

        /// <summary>
        /// 重试锁(1: 已在重试中, 0: 可以重试)
        /// </summary>
        public DateTime? RetryLock { get; set; }

        /// <summary>
        /// 立即重试
        /// </summary>
        public int RetryNow { get; set; }

        /// <summary>
        /// 重试时间
        /// </summary>
        public DateTime? RetryTime { get; set; }
        public DateTime? CheckTime { get; set; }
        public string RetryMsg { get; set; }
        public DateTime? NextRetryTime { get; set; }

        /// <summary>
        /// 最后一次执行的消息
        /// </summary>
        public BsonDocument LastedOriginalMessage { get; set; }

        public static ConsumerErrorMessage ReadFromJson(string json)
        {
            var errorMessageObject = JObject.Parse(json);
            if (errorMessageObject == null)
            {
                return null;
            }

            var originalMessage = errorMessageObject.ContainsKey(nameof(OrignalMessage)) && errorMessageObject[nameof(OrignalMessage)].HasValues
             ? JsonConvert.SerializeObject(errorMessageObject[nameof(OrignalMessage)])
             : null;

            var exception = errorMessageObject.ContainsKey(nameof(Exception)) && errorMessageObject[nameof(Exception)].HasValues
                ? errorMessageObject[nameof(Exception)]
                : null;

            var message = new ConsumerErrorMessage()
            {
                Sysid = errorMessageObject[nameof(Sysid)]?.ToString().SafeToObjectId() ?? ObjectId.Empty,
                RoutingKey = errorMessageObject[nameof(RoutingKey)]?.ToString(),
                Exchange = errorMessageObject[nameof(Exchange)]?.ToString(),
                RetryNow = 0,
                RetryLock = null
            };

            if (exception != null)
            {
                if (exception is JObject)
                {
                    message.Exception = BsonSerializer.Deserialize<BsonDocument>(exception.ToString());
                }
                else if (exception is JArray)
                {
                    message.Exception = BsonSerializer.Deserialize<BsonArray>(exception.ToString());
                }
                else
                {
                    //既不是JObject, 也不是JArray的时候的处理,理论上不存在
                    message.Exception = BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(new { ex = exception }));
                }
            }

            if (int.TryParse(errorMessageObject[nameof(ProcessTimes)]?.ToString(), out var processTimes))
            {
                message.ProcessTimes = processTimes;
            }

            if (int.TryParse(errorMessageObject[nameof(CanRetry)]?.ToString(), out var canRetry))
            {
                message.CanRetry = canRetry;
            }

            if (originalMessage.IsNotNullOrWhitespace())
            {
                message.OrignalMessage = BsonSerializer.Deserialize<BsonDocument>(originalMessage);
            }

            if (DateTime.TryParse(errorMessageObject[nameof(Logged)]?.ToString(), out var loggedTime))
            {
                message.Logged = loggedTime;
            }
            return message;
        }
    }
}
