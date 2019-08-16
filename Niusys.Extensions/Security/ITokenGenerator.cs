using Niusys.Security.Tokens;

namespace Niusys.Security
{
    public interface ITokenGenerator
    {
        string EncryptToken(AccessToken accessToken, string encryptionPrivateKey = "");
        AccessToken DecryptToken(string token, string encryptionPrivateKey = "");
    }
}
