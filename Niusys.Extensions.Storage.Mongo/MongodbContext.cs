using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Niusys.Extensions.Storage.Mongo
{
    public class MongodbContext<TSetting>
        where TSetting : MongodbOptions, new()
    {
        public MongodbContext(IOptions<TSetting> settings)
        {
            MongoSettings = settings.Value;
            MongoClientSettings mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(MongoSettings.ConnectionString));

            if (!string.IsNullOrWhiteSpace(MongoSettings.LoginDatabase))
            {
                mongoClientSettings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                var credential = MongoCredential.CreateCredential(MongoSettings.LoginDatabase, MongoSettings.UserName, MongoSettings.Password);
                mongoClientSettings.Credential = credential;
            }
            Client = new MongoClient(mongoClientSettings);
        }

        public MongoClient Client { get; }

        private MongodbOptions MongoSettings { get; }

        public IMongoDatabase GetDateBase()
        {
            return Client.GetDatabase(MongoSettings.Database, new MongoDatabaseSettings() { });
        }
    }
}
