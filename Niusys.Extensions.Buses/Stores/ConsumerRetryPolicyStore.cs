using Microsoft.Extensions.Logging;
using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.Buses
{
    public class ConsumerRetryPolicyStore : DefaultMongoStore<ConsumerRetryPolicy>, IConsumerRetryPolicyStore
    {
        public ConsumerRetryPolicyStore(MongodbContext<LogMongoSettings> mongoDatabase, ILogger<NoSqlBaseRepository<ConsumerRetryPolicy, LogMongoSettings>> logger)
            : base(mongoDatabase, logger)
        {
        }
    }
}
