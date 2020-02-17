using Niusys.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Caching.Redis
{
    public class RedisClientFactory : IRedisClientFactory
    {
        private readonly IRedisStore _redisStore;
        private readonly ILoggerFactory _loggerFactory;
        private const int DATABASE_INDEX = 0;

        public RedisClientFactory(IRedisStore redisStore, ILoggerFactory loggerFactory)
        {
            _redisStore = redisStore;
            _loggerFactory = loggerFactory;
        }

        public async Task<IRedisClient> GetClientAsync(CancellationToken cancellationToken = default)
        {
            return new RedisClient(await _redisStore.GetDatabaseAsync(DATABASE_INDEX, alwaysNewDatabase: true, token: cancellationToken), loggerFactory: _loggerFactory);
        }

        public IRedisClient GetDatabase()
        {
            return GetClientAsync().Result;
        }
    }
}
