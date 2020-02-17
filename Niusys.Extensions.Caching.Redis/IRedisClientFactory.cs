using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Caching.Redis
{
    public interface IRedisClientFactory
    {
        Task<IRedisClient> GetClientAsync(CancellationToken cancellationToken = default);
        IRedisClient GetDatabase();
    }
}
