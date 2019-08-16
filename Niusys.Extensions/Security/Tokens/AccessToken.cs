using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Niusys.Security.Tokens
{
    public class AccessToken
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int UserType { get; set; }
        public int Timestamp { get; set; }

        public ClaimContent Claims { get; set; }

        public AccessToken()
        {
            Claims = new ClaimContent();
        }

        public string Encrypt(string encryptionPrivateKey = "")
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(UserName) || UserType <= 0)
            {
                throw new Exception("Invlid data");
            }

            var ts = Utils.getTimestamp();
            var messageRawContent = Utils.pack(this.Claims);
            var signature = generateSignature(encryptionPrivateKey, UserId, UserName, UserType, ts);

            TokenPackContent packContent = new TokenPackContent(signature, UserId.GetByteArray(), UserName.GetByteArray(), (ushort)UserType, (uint)ts, messageRawContent);
            byte[] content = Utils.pack(packContent);
            return getVersion() + Utils.base64Encode(content);
        }

        public static AccessToken Decrypt(string token,string encryptionPrivateKey = "")
        {
            var version = token.Substring(0, 3);
            TokenPackContent packContent = new TokenPackContent();
            var buffer = new ByteBuf(Convert.FromBase64String(token.Substring(3)));
            packContent.unmarshal(buffer);

            var newToken = new AccessToken()
            {
                UserId = Encoding.UTF8.GetString(packContent.uid),
                UserName = Encoding.UTF8.GetString(packContent.uname),
                UserType = (int)packContent.utype,
                Timestamp = (int)packContent.ts
            };

            var claimMessage = new ClaimContent();
            claimMessage.unmarshal(new ByteBuf(packContent._messageRawContent));
            newToken.Claims = claimMessage;
            var sign = generateSignature(encryptionPrivateKey, newToken.UserId, newToken.UserName, newToken.UserType, newToken.Timestamp);
            if (Convert.ToBase64String(sign) != Convert.ToBase64String(packContent.signature))
            {
                throw new Exception("sign verify fail");
            }

            return newToken;
        }

        public static String getVersion()
        {
            return "001";
        }

        public static byte[] generateSignature(string certificate, string uid, string uname, int utype, int ts)
        {

            using (var ms = new MemoryStream())
            using (BinaryWriter baos = new BinaryWriter(ms))
            {
                baos.Write(uid.GetByteArray());
                baos.Write(uname.GetByteArray());
                baos.Write(utype);
                baos.Write(ts);
                baos.Flush();

                byte[] sign = DynamicKeyUtil.encodeHMAC(certificate, ms.ToArray(), "SHA256");
                return sign;
            }
        }
    }
}
