using MongoDB.Driver;
using Niusys.Extensions.ComponentModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Buses
{
    public interface IMongoStore<TCollection>
    {
        Task<Page<TCollection>> PaginationSearchAsync(FilterDefinition<TCollection> filter, SortDefinition<TCollection> sortDefinition, int pageIndex, int pageSize, bool ignoreCount = true, long defaultCountNumber = 10000, CancellationToken cancellationToken = default);
        Task AddAsync(TCollection message, InsertOneOptions options = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(FilterDefinition<TCollection> filterDefinition, UpdateDefinition<TCollection> updateDefinition, CancellationToken cancellationToken = default);
        Task<IList<TCollection>> SearchAsync(FilterDefinition<TCollection> lockedFilter, SortDefinition<TCollection> sort, int pageCount, CancellationToken cancellationToken = default);
        Task<TCollection> SearchOneAsync(FilterDefinition<TCollection> filterDefinition, CancellationToken cancellationToken = default);
        Task<long> UpdateManyAsync(FilterDefinition<TCollection> filter, UpdateDefinition<TCollection> updateDefinition, CancellationToken cancellationToken = default);
        Task<bool> Delete(string messageId, CancellationToken cancellationToken = default);
        Task<TCollection> GetByIdAsync(string messageId, CancellationToken cancellationToken = default);
    }
}
