using Niusys;
using System.Collections.Generic;
using System.Reflection;

namespace Niusys.Extensions.Storage.Mongo
{
    internal class MongoCollectionNameCache
    {
        private static SortedDictionary<string, string> _dicCollectionNameCache = new SortedDictionary<string, string>();
        private static object _addLocker = new object();
        public static string GetCollectionName<TEntity>()
        {
            var typeName = typeof(TEntity).Name;
            if (_dicCollectionNameCache.TryGetValue(typeName, out var collectionName) && collectionName.IsNotNullOrWhitespace())
            {
                return collectionName;
            }
            else
            {
                lock (_addLocker)
                {
                    if (_dicCollectionNameCache.ContainsKey(typeName))
                        return _dicCollectionNameCache[typeName];

                    var attr = typeof(TEntity).GetCustomAttribute<MongoCollectionAttribute>(false);
                    collectionName = attr != null && attr.CollectionName.IsNotNullOrWhitespace() ? attr.CollectionName : typeof(TEntity).Name;

                    _dicCollectionNameCache.Add(typeof(TEntity).Name, collectionName);
                    return collectionName;
                }
            }
        }
    }
}
