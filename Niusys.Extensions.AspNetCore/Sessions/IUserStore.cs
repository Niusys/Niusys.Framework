using MongoDB.Bson;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.Sessions
{
    public interface IUserStore<TUser>
    {
        Task<TUser> GetByPropertyAsync<TField>(Expression<Func<TUser, TField>> expression, TField value);
        object GetByPropertyAsync<TUser>(Func<TUser, ObjectId> p1, object p2) where TUser : class, IUser;
    }
}