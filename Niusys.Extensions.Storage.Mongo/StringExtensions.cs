using MongoDB.Bson;

namespace Niusys.Extensions.Storage.Mongo
{
    public static class StringExtensions
    {
        public static ObjectId SafeToObjectId(this string strObjectId)
        {
            return ObjectId.TryParse(strObjectId, out var objectId) ? objectId : ObjectId.Empty;
        }
    }
}
