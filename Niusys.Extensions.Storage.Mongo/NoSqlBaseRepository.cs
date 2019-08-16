using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.Storage.Mongo
{
    public abstract class NoSqlBaseRepository<TEntity, TMongoSetting> : MongoKeylessBaseRepository<TEntity, TMongoSetting>
        where TEntity : IMongoEntity<ObjectId>
        where TMongoSetting : MongodbOptions, new()
    {
        private readonly ILogger<NoSqlBaseRepository<TEntity, TMongoSetting>> _logger;

        public NoSqlBaseRepository(MongodbContext<TMongoSetting> mongoDatabase, ILogger<NoSqlBaseRepository<TEntity, TMongoSetting>> logger)
            : base(mongoDatabase, logger)
        {
            this._logger = logger;
        }

        public async Task<bool> Update(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Begin Update");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, entity.Sysid);
            var result = await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
            _logger.LogTrace("End Update");
            return result.ModifiedCount == 1;
        }

        public async Task<bool> ReplaceOneAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Begin ReplaceOneAsync");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, entity.Sysid);
            var result = (await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken)).ModifiedCount == 1;
            _logger.LogTrace("End ReplaceOneAsync");
            return result;
        }

        public async Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Begin GetByIdAsync");
            var result = await Collection.Find(Builders<TEntity>.Filter.Eq(x => x.Sysid, id.SafeToObjectId())).FirstOrDefaultAsync(cancellationToken);
            _logger.LogTrace("End GetByIdAsync");
            return result;
        }

        public async Task<bool> Delete(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Begin Delete");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, entity.Sysid);
            var result = (await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)).DeletedCount > 0;
            _logger.LogTrace("End Delete");
            return result;
        }

        public async Task<bool> Delete(string sysid, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogTrace("Begin Delete");
            var filter = Builders<TEntity>.Filter.Eq(x => x.Sysid, sysid.SafeToObjectId());
            var result = (await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)).DeletedCount > 0;
            _logger.LogTrace("End Delete");
            return result;
        }
    }
}
