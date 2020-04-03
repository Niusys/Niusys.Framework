using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Niusys.Extensions.Storage.PostgreSql
{
    public class SqlConnectionManager
    {
        public SqlConnectionManager(IConfiguration configuration, IOptions<DatabaseOptions> dbSettingOptions, ILogger<SqlConnectionManager> logger)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.DbSetting = (dbSettingOptions ?? throw new ArgumentNullException(nameof(dbSettingOptions))).Value;
        }

        public IConfiguration Configuration { get; }
        public DatabaseOptions DbSetting { get; }

        public NpgsqlConnection GetNewConnection(string connectionName = "Default")
        {
            var connectionString = GetConnectionString(connectionName);
            var conn = new NpgsqlConnection(connectionString);
            return conn;
        }

        public string GetConnectionString(string connectionName)
        {
            var connectionString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception($"未找到Name为{connectionName}的配置项或者对应配置项为空");

            if (!string.IsNullOrWhiteSpace(DbSetting.DecryptKey))
            {
                string str = MD5Encrypt(DbSetting.DecryptKey).Substring(0, 8);
                connectionString = DESDecrypt(connectionString, str, str);
            }
            return connectionString;
        }

        #region 数据库连接字符串加解密
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="plaintext">明文</param>
        /// <returns>加密后的密文</returns>
        private static string MD5Encrypt(string plaintext)
        {
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            var sb = new StringBuilder();
            foreach (byte t in data)
            {
                sb.AppendFormat("{0:X2}", t);
            }
            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// DES解密
        /// <para>解密使用的是UTF8格式</para>
        /// </summary>
        /// <param name="ciphertext">密文</param>
        /// <param name="decryptKey">解密KEY</param>
        /// <param name="IV_64">64KEY</param>
        /// <returns>解密后的字符串</returns>
        private static string DESDecrypt(string ciphertext, string decryptKey, string IV_64)
        {
            DES provider = DES.Create();
            provider.Key = Encoding.UTF8.GetBytes(decryptKey);
            provider.IV = Encoding.UTF8.GetBytes(IV_64);
            byte[] buffer = new byte[ciphertext.Length / 2];
            for (int i = 0; i < (ciphertext.Length / 2); i++)
            {
                int num2 = Convert.ToInt32(ciphertext.Substring(i * 2, 2), 0x10);
                buffer[i] = (byte)num2;
            }
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, provider.CreateDecryptor(), CryptoStreamMode.Write);
            stream2.Write(buffer, 0, buffer.Length);
            stream2.FlushFinalBlock();
            stream.Close();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        #endregion
    }
}
