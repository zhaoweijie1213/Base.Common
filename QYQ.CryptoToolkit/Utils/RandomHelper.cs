using System;
using System.Security.Cryptography;

namespace QYQ.CryptoToolkit.Utils
{
    /// <summary>
    /// 随机数辅助类
    /// </summary>
    public static class RandomHelper
    {
        /// <summary>
        /// 生成随机 Base64 编码密钥
        /// </summary>
        /// <param name="bitLength">密钥长度（支持 128、192、256）</param>
        /// <returns>Base64 编码的密钥</returns>
        public static string GenerateRandomBase64Key(int bitLength)
        {
            byte[] key = new byte[bitLength / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// 生成随机字节数组
        /// </summary>
        /// <param name="length">字节数组长度</param>
        /// <returns>随机字节数组</returns>
        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }
    }
}
