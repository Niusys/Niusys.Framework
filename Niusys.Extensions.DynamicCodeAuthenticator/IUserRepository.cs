using System.Threading.Tasks;

namespace Niusys.Extensions.DynamicCodeAuthenticator
{
    public interface IUserRepository<TUser>
        where TUser : IDynamicCodeUser
    {
        Task ResetUserDynamicCodeSecret(TUser user, string gaSecret);
        Task DisableUserDynamicCodeStatus(TUser user);
        Task EnableUserDynamicCodeStatus(TUser user);
    }
}
