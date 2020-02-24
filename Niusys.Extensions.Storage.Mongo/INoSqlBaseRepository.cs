using Niusys.Extensions.ComponentModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Niusys.Extensions.Storage.Mongo
{
    public interface INoSqlBaseRepository<TEntity> where TEntity : MongoEntity
    {
        IMongoCollection<TEntity> Collection { get; }
        Task AddAsync(TEntity entity, InsertOneOptions options = null, CancellationToken cancellationToken = default);
        Task AddManyAsync(List<TEntity> items, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAllAsync(CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(ObjectId sysid, CancellationToken cancellationToken = default);
        Task DeleteManyAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
        Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetByPropertyAsync<TField>(Expression<Func<TEntity, TField>> expression, TField value, CancellationToken cancellationToken = default);
        Task<Page<TEntity>> PaginationSearchAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, int pageIndex = 1, int pageSize = 20, bool ignoreCount = true, long defaultCountNumber = 10000, CancellationToken cancellationToken = default);
        Task<bool> ReplaceOneAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
        Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, int limit, CancellationToken cancellationToken = default);
        Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, CancellationToken cancellationToken = default);
        Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, int limit, int skip = 0, CancellationToken cancellationToken = default);
        Task<TEntity> SearchOneAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> expression, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default);
        Task<long> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default);
    }
}
