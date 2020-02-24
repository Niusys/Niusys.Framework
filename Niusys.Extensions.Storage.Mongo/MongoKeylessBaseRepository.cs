using Niusys.Extensions.ComponentModels;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Storage.Mongo
{
    public abstract class MongoKeylessBaseRepository<TEntity, TMongoSetting>
         where TMongoSetting : MongodbOptions, new()
    {
        public IMongoCollection<TEntity> Collection { get; }
        private readonly ILogger _logger;

        public MongoKeylessBaseRepository(MongodbContext<TMongoSetting> mongoDatabase, ILogger logger, MongoCollectionSettings mongoCollectionSettings = default)
        {
            var mongoDatabase1 = mongoDatabase.GetDateBase();
            var collectionName = MongoCollectionNameCache.GetCollectionName<TEntity>();
            Collection = mongoDatabase1.GetCollection<TEntity>(collectionName, mongoCollectionSettings);
            _logger = logger;
        }
        public virtual async Task Add(TEntity entity, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Add");
            await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
            _logger.LogTrace("End Add");
        }

        public virtual async Task AddAsync(TEntity entity, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin AddAsync");
            await Collection.InsertOneAsync(entity, options: options, cancellationToken: cancellationToken);
            _logger.LogTrace("End AddAsync");
        }

        public virtual async Task AddManyAsync(List<TEntity> items, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin AddManyAsync");
            await Collection.InsertManyAsync(items, cancellationToken: cancellationToken);
            _logger.LogTrace("End AddManyAsync");
        }

        public async Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> expression, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin UpdateManyAsync");
            var result = await Collection.UpdateManyAsync<TEntity>(expression, updateDefinition, cancellationToken: cancellationToken);
            _logger.LogTrace("End UpdateManyAsync");
            return result.ModifiedCount;
        }

        public async Task<bool> UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin UpdateAsync");
            var result = await Collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
            _logger.LogTrace("End UpdateAsync");
            return result.ModifiedCount == 1;
        }

        public async Task<long> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin UpdateManyAsync");
            var result = await Collection.UpdateManyAsync(filter, updateDefinition, cancellationToken: cancellationToken);
            _logger.LogTrace("End UpdateManyAsync");
            return result.ModifiedCount;
        }

        public async Task DeleteManyAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin DeleteManyAsync");
            await Collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);
            _logger.LogTrace("End DeleteManyAsync");
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin DeleteAllAsync");
            await Collection.DeleteManyAsync(Builders<TEntity>.Filter.Empty, cancellationToken: cancellationToken);
            _logger.LogTrace("End DeleteAllAsync");
        }

        public async Task<IList<TEntity>> Search(Expression<Func<TEntity, bool>> predicate, SortDefinition<TEntity> sort, int limit, int skip = 0, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Search");
            var records = await Collection.Find(predicate).Sort(sort).Limit(limit).Skip(skip).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End Search");
            return records;
        }

        public async Task<IList<TEntity>> Search(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, int limit, int skip = 0, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Search");
            var records = await Collection.Find(filter).Sort(sort).Limit(limit).Skip(skip).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End Search");
            return records;
        }

        public async Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin SearchAsync");
            var result = await Collection.Find(filter).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End SearchAsync");
            return result;
        }

        public async Task<long> CountAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin CountAsync");
            var result = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            _logger.LogTrace("End CountAsync");
            return result;
        }

        public async Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin SearchAsync");
            var result = await Collection.Find(filter).Sort(sort).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End SearchAsync");
            return result;
        }

        public async Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, int pageCount, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin SearchAsync");
            var result = await Collection.Find(filter).Sort(sort).Limit(pageCount).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End SearchAsync");
            return result;
        }

        public async Task<IList<TEntity>> SearchAsync(FilterDefinition<TEntity> filter, int pageCount, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin SearchAsync");
            var result = await Collection.Find(filter).Limit(pageCount).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End SearchAsync");
            return result;
        }

        public async Task<TEntity> SearchOneAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin SearchOneAsync");
            var result = await Collection.Find(filter).SingleOrDefaultAsync(cancellationToken);
            _logger.LogTrace("End SearchOneAsync");
            return result;
        }

        public async Task<bool> ExistsAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin ExistsAsync");
            var result = await Collection.CountDocumentsAsync(filter);
            _logger.LogTrace("End ExistsAsync");
            return result > 0;
        }

        public async Task<Page<TEntity>> PaginationSearchAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, int pageIndex = 1, int pageSize = 20, bool ignoreCount = true, long defaultCountNumber = 10000, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin PaginationSearchAsync");

            //PageIndex设定为从1开始，不能小于1
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 20;
            }

            var searchTask = Collection.Find(filter).Sort(sort).Limit(pageSize).Skip((pageIndex - 1) * pageSize).ToListAsync(cancellationToken: cancellationToken);
            var totalTask = ignoreCount ? Task.FromResult(defaultCountNumber) : Collection.CountDocumentsAsync(filter, new CountOptions { Limit = defaultCountNumber }, cancellationToken: cancellationToken);

            Task.WaitAll(searchTask, totalTask);

            var result = await Task.FromResult(new Page<TEntity>()
            {
                Records = searchTask.Result,
                Paging = new Paging() { Total = (int)totalTask.Result, PageIndex = pageIndex, PageSize = pageSize }
            });
            _logger.LogTrace("End PaginationSearchAsync");
            return result;
        }

        public async Task<IList<TEntity>> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin GetAll");
            var filter = Builders<TEntity>.Filter.Empty;
            var result = await Collection.Find(filter).ToListAsync(cancellationToken: cancellationToken);
            _logger.LogTrace("End GetAll");
            return result;
        }

        public async Task<bool> Delete(TEntity entity, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Delete");
            var result = (await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)).DeletedCount > 0;
            _logger.LogTrace("End Delete");
            return result;
        }
    }
}
