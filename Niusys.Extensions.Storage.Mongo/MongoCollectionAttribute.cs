using System;

namespace Niusys.Extensions.Storage.Mongo
{
    public class MongoCollectionAttribute : Attribute
    {
        public string CollectionName { get; set; }
    }
}
