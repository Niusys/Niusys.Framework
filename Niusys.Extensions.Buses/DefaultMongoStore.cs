using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.Buses
{
    public class DefaultMongoStore<TCollection> : NoSqlBaseRepository<TCollection, LogMongoSettings>, IMongoStore<TCollection>
        where TCollection : MongoEntity, IMongoEntity<ObjectId>
    {
        public DefaultMongoStore(MongodbContext<LogMongoSettings> mongoDatabase,
            ILogger<NoSqlBaseRepository<TCollection, LogMongoSettings>> logger) : base(mongoDatabase, logger)
        {
        }
    }
}
