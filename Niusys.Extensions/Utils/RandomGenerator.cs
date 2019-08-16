using System;

namespace Niusys.Utils
{
    public static class RandomGenerator
    {
        #region BaseAlgorithm
        /// <summary>
        /// 创建随机序列
        /// </summary>
        public static string CreateRandomSequence(string head, int length)
        {
            char[] chars = {
                                 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I','J', 'K', 'L','M', 'N', 'O','P', 'Q', 'R', 'S',
                                 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0','1', '2', '3', '4', '5', '6', '7', '8', '9'
                             };

            long epoch = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

            for (int i = 0; i < length; i++)
            {
                Random rnd = new Random(GetRandomSeed());
                head += chars[rnd.Next(0, 35)].ToString();
            }

            return head;
        }

        public static string CreateRandomSequence(string head, int length, int flag)
        {
            char[] chars = {
                                  'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i','j', 'k', 'l','m', 'n', 'o','p', 'q', 'r', 's',
                                 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I','J', 'K', 'L','M', 'N', 'O','P', 'Q', 'R', 'S',
                                 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0','1', '2', '3', '4', '5', '6', '7', '8', '9'
                             };

            long epoch = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

            for (int i = 0; i < length; i++)
            {
                Random rnd = new Random(GetRandomSeed());
                head += chars[rnd.Next(0, chars.Length - 1)].ToString();
            }

            return head;
        }

        /// <summary>
        /// 加密随机数生成器 生成随机种子
        /// </summary>
        private static int GetRandomSeed()
        {

            byte[] bytes = new byte[4];

            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();

            rng.GetBytes(bytes);

            return BitConverter.ToInt32(bytes, 0);

        }
        #endregion
    }
}
