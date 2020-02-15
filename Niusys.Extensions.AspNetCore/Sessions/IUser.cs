using MongoDB.Bson;

namespace Niusys.Extensions.AspNetCore.Sessions
{
    public interface IUser
    {
        public ObjectId Sysid { get; set; }
    }
}