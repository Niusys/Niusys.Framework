using Niusys.Security.Tokens;
using Microsoft.Extensions.Options;

namespace Niusys.Security
{
    public class DefaultTokenGenerator : ITokenGenerator
    {
        #region Fields

        private readonly SecurityOptions _securitySettings;

        #endregion

        #region Ctor

        public DefaultTokenGenerator(IOptions<SecurityOptions> securitySettingsOptions)
        {
            _securitySettings = securitySettingsOptions.Value;
        }

        #endregion
        public AccessToken DecryptToken(string token, string encryptionPrivateKey = "")
        {
            if (string.IsNullOrEmpty(encryptionPrivateKey))
            {
                encryptionPrivateKey = _securitySettings.EncryptionKey;
            }
            return AccessToken.Decrypt(token, encryptionPrivateKey);
        }

        public string EncryptToken(AccessToken accessToken, string encryptionPrivateKey = "")
        {
            if (string.IsNullOrEmpty(encryptionPrivateKey))
            {
                encryptionPrivateKey = _securitySettings.EncryptionKey;
            }
            return accessToken.Encrypt(encryptionPrivateKey);
        }
    }
}
