using System;
using System.Security.Cryptography;
using System.Text;

namespace QYQ.CryptoToolkit.Asymmetric
{
    /// <summary>
    /// AES-CBC 实现
    /// </summary>
    public static class AesCbcEncryptor
    {

        /// <summary>
        /// 生成 AES-CBC 密钥和 IV（默认 128 位）
        /// </summary>
        /// <param name="keySizeInBits">密钥长度，支持 128 / 192 / 256</param>
        /// <returns>返回 Base64 格式的 Key 和 IV</returns>
        public static (string, string) GenerateKey(int keySizeInBits = 256)
        {
            using (var aes = CreateAes())
            {
                aes.KeySize = keySizeInBits;

                aes.GenerateKey();
                aes.GenerateIV();

                string base64Key = Convert.ToBase64String(aes.Key);
                string base64IV = Convert.ToBase64String(aes.IV);

                return (base64Key, base64IV);
            }
        }

        /// <summary>
        /// AES CBC 加密
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="base64Key">Base64 编码的密钥</param>
        /// <param name="base64IV">Base64 编码的 IV</param>
        /// <returns>Base64 编码的密文</returns>
        public static string Encrypt(string plainText, string base64Key, string base64IV)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] iv = Convert.FromBase64String(base64IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);

            using (var aes = CreateAes())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
        }

        /// <summary>
        /// AES CBC 解密
        /// </summary>
        /// <param name="base64Cipher">Base64 编码的密文</param>
        /// <param name="base64Key">Base64 编码的密钥</param>
        /// <param name="base64IV">Base64 编码的 IV</param>
        /// <returns>明文字符串</returns>
        public static string Decrypt(string base64Cipher, string base64Key, string base64IV)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] iv = Convert.FromBase64String(base64IV);
            byte[] cipherBytes = Convert.FromBase64String(base64Cipher);

            using (var aes = CreateAes())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        /// <summary>
        /// 创建 AES 实例（兼容多框架）
        /// </summary>
        /// <returns>AES 实例</returns>
        private static Aes CreateAes()
        {
#if NETSTANDARD2_0
            return new AesManaged();
#else
            return Aes.Create();
#endif
        }
    }
}
