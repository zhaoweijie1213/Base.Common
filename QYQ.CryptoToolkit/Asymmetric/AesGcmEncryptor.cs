using QYQ.CryptoToolkit.Utils;
using System;
using System.Security.Cryptography;
using System.Text;

namespace QYQ.CryptoToolkit.Asymmetric
{
    /// <summary>
    /// AES-GCM 实现
    /// </summary>
    public static class AesGcmEncryptor
    {
        /// <summary>
        /// 生成 AES 密钥（128/192/256 位），Base64 编码
        /// </summary>
        /// <param name="bitLength">密钥位数</param>
        /// <returns>Base64 编码的密钥</returns>
        public static string GenerateKey(int bitLength = 128)
        {
            if (bitLength != 128 && bitLength != 192 && bitLength != 256)
                throw new ArgumentException("只支持 128、192 或 256 位密钥");

            return RandomHelper.GenerateRandomBase64Key(bitLength);
        }

        /// <summary>
        /// AES GCM 加密
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="base64Key">Base64 编码的密钥</param>
        /// <returns>Base64 编码的密文（包含随机 IV 和 Tag）</returns>
        public static string Encrypt(string plainText, string base64Key)
        {
#if NETSTANDARD2_0
            throw new PlatformNotSupportedException("AES-GCM 需要 .NET Standard 2.1 或更高版本。");
#else
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] nonce = RandomHelper.GenerateRandomBytes(12);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[16]; // 128-bit tag

            using var aes = CreateAesGcm(key);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            byte[] encryptedBytes = new byte[nonce.Length + cipherBytes.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, encryptedBytes, 0, nonce.Length);
            Buffer.BlockCopy(cipherBytes, 0, encryptedBytes, nonce.Length, cipherBytes.Length);
            Buffer.BlockCopy(tag, 0, encryptedBytes, nonce.Length + cipherBytes.Length, tag.Length);

            return Convert.ToBase64String(encryptedBytes);
#endif
        }

        /// <summary>
        /// AES GCM 解密
        /// </summary>
        /// <param name="base64CipherText">Base64 编码的密文（包含随机 IV 和 Tag）</param>
        /// <param name="base64Key">Base64 编码的密钥</param>
        /// <returns>明文字符串</returns>
        public static string Decrypt(string base64CipherText, string base64Key)
        {
#if NETSTANDARD2_0
            throw new PlatformNotSupportedException("AES-GCM 需要 .NET Standard 2.1 或更高版本。");
#else
            byte[] encryptedBytes = Convert.FromBase64String(base64CipherText);
            byte[] key = Convert.FromBase64String(base64Key);

            byte[] nonce = new byte[12];
            byte[] tag = new byte[16];
            byte[] cipherBytes = new byte[encryptedBytes.Length - nonce.Length - tag.Length];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptedBytes, nonce.Length, cipherBytes, 0, cipherBytes.Length);
            Buffer.BlockCopy(encryptedBytes, nonce.Length + cipherBytes.Length, tag, 0, tag.Length);

            byte[] plainBytes = new byte[cipherBytes.Length];

            try
            {
                using var aes = CreateAesGcm(key);
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException("解密失败，数据可能被篡改或密钥错误", ex);
            }
#endif
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// 创建 AesGcm 实例，兼容 .NET 8.0+
        /// </summary>
        /// <param name="key">加密密钥</param>
        /// <returns>AesGcm 实例</returns>
        private static AesGcm CreateAesGcm(byte[] key)
        {
#if NET8_0_OR_GREATER
            return new AesGcm(key , tagSizeInBytes: 128);
#else
            return new AesGcm(key);
#endif
        }
#endif
    }
}
