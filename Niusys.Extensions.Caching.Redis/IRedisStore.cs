using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Niusys.Extensions.Caching.Redis
{
    public interface IRedisStore
    {
        Task<IConnectionMultiplexer> GetConnectionMultiplexer(bool allowAdmin = false, CancellationToken token = default);
        //IDatabase GetDatabase(int databaseIndex = 0);
        Task<IDatabase> GetDatabaseAsync(int databaseIndex = 0, bool alwaysNewDatabase = false, CancellationToken token = default);
    }
}
