using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Storage.Mongo
{
    public abstract class NoSqlBaseRepository<TEntity, TMongoSetting> : MongoKeylessBaseRepository<TEntity, TMongoSetting>, INoSqlBaseRepository<TEntity>
        where TEntity : MongoEntity, IMongoEntity<ObjectId>
        where TMongoSetting : MongodbOptions, new()
    {
        private readonly ILogger _logger;

        public NoSqlBaseRepository(MongodbContext<TMongoSetting> mongoDatabase, ILogger logger)
            : base(mongoDatabase, logger)
        {
            this._logger = logger;
        }

        public virtual async Task<TEntity> GetByPropertyAsync<TField>(Expression<Func<TEntity, TField>> expression, TField value, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(expression, value);
            return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Update");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, entity.Sysid);
            var result = await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
            _logger.LogTrace("End Update");
            return result.ModifiedCount == 1;
        }

        public virtual async Task<bool> ReplaceOneAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin ReplaceOneAsync");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, entity.Sysid);
            var result = (await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken)).ModifiedCount == 1;
            _logger.LogTrace("End ReplaceOneAsync");
            return result;
        }

        public virtual async Task<TEntity> GetByIdAsync(ObjectId sysId, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin GetByIdAsync");
            var result = await Collection.Find(Builders<TEntity>.Filter.Eq(x => x.Sysid, sysId)).FirstOrDefaultAsync(cancellationToken);
            _logger.LogTrace("End GetByIdAsync");
            return result;
        }

        public virtual async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Delete");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, entity.Sysid);
            var result = (await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)).DeletedCount > 0;
            _logger.LogTrace("End Delete");
            return result;
        }

        public virtual async Task<bool> DeleteAsync(ObjectId sysid, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Begin Delete");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, sysid);
            var result = (await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)).DeletedCount > 0;
            _logger.LogTrace("End Delete");
            return result;
        }
    }
}
