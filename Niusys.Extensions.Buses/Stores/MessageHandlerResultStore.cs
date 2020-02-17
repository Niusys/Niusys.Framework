using Microsoft.Extensions.Logging;
using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.Buses
{
    public class MessageHandlerResultStore : DefaultMongoStore<MessageHandlerResult>, IMessageHandlerResultStore
    {
        public MessageHandlerResultStore(MongodbContext<LogMongoSettings> mongoDatabase, ILogger<NoSqlBaseRepository<MessageHandlerResult, LogMongoSettings>> logger) 
            : base(mongoDatabase, logger)
        {
        }
    }
}
