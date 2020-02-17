using Microsoft.Extensions.Logging;
using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.Buses
{
    public class ConsumerErrorMessageStore : DefaultMongoStore<ConsumerErrorMessage>, IConsumerErrorMessageStore
    {
        public ConsumerErrorMessageStore(MongodbContext<LogMongoSettings> mongoDatabase, ILogger<NoSqlBaseRepository<ConsumerErrorMessage, LogMongoSettings>> logger) 
            : base(mongoDatabase, logger)
        {
        }
    }
}
