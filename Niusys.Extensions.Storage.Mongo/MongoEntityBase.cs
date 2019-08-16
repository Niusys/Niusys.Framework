using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Niusys.Extensions.Storage.Mongo
{
    public class MongoEntity : IMongoEntity<ObjectId>
    {
        public MongoEntity()
        {
            //Sysid = ObjectId.Empty;
        }

        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Sysid { get; set; }
    }
}
