using System;
using System.Security.Cryptography;
using System.Text;

namespace QYQ.CryptoToolkit.Asymmetric
{
    /// <summary>
    /// DES-ECB 实现
    /// </summary>
    public static class DesEcbEncryptor
    {
        /// <summary>
        /// 生成 DES 密钥（ECB 模式不需要 IV）
        /// </summary>
        /// <returns>返回 Base64 格式的 Key</returns>
        public static string GenerateKey()
        {
            using (var des = CreateDes())
            {
                des.GenerateKey();
                string base64Key = Convert.ToBase64String(des.Key);
                return base64Key;
            }
        }

        /// <summary>
        /// DES ECB 加密
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="base64Key">Base64 编码的密钥</param>
        /// <returns>Base64 编码的密文</returns>
        public static string Encrypt(string plainText, string base64Key)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);

            using (var des = CreateDes())
            {
                des.Key = key;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.Zeros;

                using (var encryptor = des.CreateEncryptor())
                {
                    byte[] cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
        }

        /// <summary>
        /// DES ECB 解密
        /// </summary>
        /// <param name="base64Cipher">Base64 编码的密文</param>
        /// <param name="base64Key">Base64 编码的密钥</param>
        /// <returns>明文字符串</returns>
        public static string Decrypt(string base64Cipher, string base64Key)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] cipherBytes = Convert.FromBase64String(base64Cipher);

            using (var des = CreateDes())
            {
                des.Key = key;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.Zeros;

                using (var decryptor = des.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        /// <summary>
        /// 创建 DES 实例（兼容多框架）
        /// </summary>
        /// <returns>DES 实例</returns>
        private static DES CreateDes()
        {
#if NETSTANDARD2_0
            return new DESCryptoServiceProvider();
#else
            return DES.Create();
#endif
        }
    }
}
