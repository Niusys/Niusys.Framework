using MongoDB.Driver;
using Niusys.Extensions.ComponentModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Buses
{
    public abstract class NullMongoStore<TCollection> : IMongoStore<TCollection>
    {
        public Task AddAsync(TCollection message, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteAsync(string messageId, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<TCollection> GetByIdAsync(string messageId, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<Page<TCollection>> PaginationSearchAsync(FilterDefinition<TCollection> filter, SortDefinition<TCollection> sortDefinition, int pageIndex, int pageSize, bool ignoreCount = true, long defaultCountNumber = 10000, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<TCollection>> SearchAsync(FilterDefinition<TCollection> lockedFilter, SortDefinition<TCollection> sort, int pageCount, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<TCollection> SearchOneAsync(FilterDefinition<TCollection> filterDefinition, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> UpdateAsync(FilterDefinition<TCollection> filterDefinition, UpdateDefinition<TCollection> updateDefinition, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<long> UpdateManyAsync(FilterDefinition<TCollection> filter, UpdateDefinition<TCollection> updateDefinition, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
