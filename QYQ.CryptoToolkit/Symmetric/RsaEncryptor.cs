using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace QYQ.CryptoToolkit.Symmetric
{
    /// <summary>
    /// RSA 加解密工具类
    /// </summary>
    public static class RsaEncryptor
    {

        // <summary>
        /// 生成 RSA 密钥对（Base64 编码）
        /// <param name="keySize">密钥长度（如：2048, 4096）</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string publicKey, string privateKey) GenerateKey(int keySize)
        {
#if NETSTANDARD2_0
            throw new PlatformNotSupportedException("GenerateKey 方法需要 .NET Standard 2.1 或更高版本。");
#else
            using (var rsa = RSA.Create(keySize))
            {
                // 导出公钥和私钥（Base64 编码）
                var privateKeyBytes = rsa.ExportRSAPrivateKey();
                var publicKeyBytes = rsa.ExportRSAPublicKey();

                string privateKey = Convert.ToBase64String(privateKeyBytes);
                string publicKey = Convert.ToBase64String(publicKeyBytes);

                return (publicKey, privateKey);
            }
#endif
        }

        /// <summary>
        /// RSA 公钥加密
        /// </summary>
        /// <param name="originalData"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string Encrypt(string originalData, string publicKey)
        {
#if NETSTANDARD2_0
            throw new PlatformNotSupportedException("GenerateKey 方法需要 .NET Standard 2.1 或更高版本。");
#else
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                byte[] originalDataBytes = Encoding.UTF8.GetBytes(originalData);
                byte[] encryptedDataBytes = rsa.Encrypt(originalDataBytes, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(encryptedDataBytes);

            }
#endif
        }

        /// <summary>
        /// RSA 私钥解密
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptedData, string privateKey)
        {
#if NETSTANDARD2_0
            throw new PlatformNotSupportedException("GenerateKey 方法需要 .NET Standard 2.1 或更高版本。");
#else
            using (RSA rsa = RSA.Create())
            {

                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

                byte[] encryptedDataBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedDataBytes = rsa.Decrypt(encryptedDataBytes, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedDataBytes);
            }
#endif
        }

        /// <summary>
        /// RSA 公钥加密（.NET Standard 2.0 兼容）
        /// </summary>
        /// <param name="originalData">原始字符串</param>
        /// <param name="publicKey">Base64 编码的 XML 公钥</param>
        /// <returns>Base64 编码的密文</returns>
        public static string EncryptNetStandard2(string originalData, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(publicKey);

                    byte[] dataToEncrypt = Encoding.UTF8.GetBytes(originalData);
                    byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false); // false = PKCS#1 v1.5 padding
                    return Convert.ToBase64String(encryptedData);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        /// <summary>
        /// RSA 私钥解密（.NET Standard 2.0 兼容）
        /// </summary>
        /// <param name="encryptedData">Base64 编码的密文</param>
        /// <param name="privateKey">Base64 编码的 XML 私钥</param>
        /// <returns>解密后的原文字符串</returns>
        public static string DecryptNetStandard2(string encryptedData, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(privateKey);

                    byte[] dataToDecrypt = Convert.FromBase64String(encryptedData);
                    byte[] decryptedData = rsa.Decrypt(dataToDecrypt, false);
                    return Encoding.UTF8.GetString(decryptedData);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

    }
}
