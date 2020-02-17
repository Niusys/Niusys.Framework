using MongoDB.Bson.Serialization.Attributes;
using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.Buses
{
    /// <summary>
    /// ConsumerRetryPolicy
    /// </summary>
    [BsonIgnoreExtraElements]
    public class ConsumerRetryPolicy : MongoEntity
    {
        /// <summary>
        /// RoutingKey
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// AllowRetry
        /// </summary>
        public int AllowRetry { get; set; }
    }
}
