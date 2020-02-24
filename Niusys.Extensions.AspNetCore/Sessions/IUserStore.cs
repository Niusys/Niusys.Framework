using MongoDB.Bson;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.Sessions
{
    public interface IUserStore<TUser>
    {
        Task<TUser> GetByPropertyAsync<TField>(Expression<Func<TUser, TField>> expression, TField value, CancellationToken cancellationToken = default);
    }
}