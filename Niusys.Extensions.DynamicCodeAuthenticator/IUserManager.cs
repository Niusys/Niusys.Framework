using System.Threading.Tasks;

namespace Niusys.Extensions.DynamicCodeAuthenticator
{
    public interface IUserManager<TDynamicCodeUser>
        where TDynamicCodeUser : IDynamicCodeUser
    {
        Task<byte[]> CreateSecurityTokenAsync(TDynamicCodeUser user);
        string GenerateNewAuthenticatorKey();
        Task<string> GetAuthenticatorKeyAsync(IDynamicCodeUser user);
        Task<string> GetSecurityStampAsync(TDynamicCodeUser user);
        Task<string> GetUserNameAsync(TDynamicCodeUser user);
        Task ResetAuthenticatorKeyAsync(TDynamicCodeUser user);
        Task SetGaEnabledAsync(TDynamicCodeUser user, bool enabled);
        Task<bool> VerifyDynamicCodeTokenAsync(TDynamicCodeUser user, string token);
        Task<AuthenticatorModel> GenerateSharedKeyAndQrCodeUriAsync(TDynamicCodeUser playerGaWrapper);
    }

    public class AuthenticatorModel
    {
        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }
    }
}