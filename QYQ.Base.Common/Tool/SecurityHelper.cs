using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Tool
{
    /// <summary>
    /// 算法相关
    /// </summary>
    public class SecurityHelper
    {

        #region HMAC SHA256 (Base64)

        /// <summary>
        /// HMAC SHA256 (Base64)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string GetHMACSHA256Base64(string message, string secret)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }

        /// <summary>
        /// C# HMAC SHA256 (64位原始)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string GetHMACSHA256(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            StringBuilder builder = new();
            for (int i = 0; i < hashmessage.Length; i++)
            {
                builder.Append(hashmessage[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// string转hex
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }

        #endregion

        #region MD5

        /// <summary>
        /// MD5字符串加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns>加密后字符串</returns>
        public static string GetMD5Hash(string txt)
        {
            //创建默认实现的实例
            byte[] hash;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(txt));
            }
            // Now convert the binary hash into text if you must...
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        #endregion

        #region AES-128/GCM + BASE64

        /// <summary>
        /// 加密 AES-128/GCM + BASE64
        /// </summary>
        /// <param name="plainText">待加密字符串</param>
        /// <param name="base64Key"></param>
        /// <returns></returns>
        public static string EncryptByAES128AndGCM(string plainText, string base64Key)
        {
            byte[] key = Convert.FromBase64String(base64Key);

            using AesGcm aesGcm = new(key);

            byte[] nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce); // 生成随机的 12 字节 nonce
            //加密后的字节
            byte[] cipherText = new byte[plainText.Length];
            //标签
            byte[] tag = new byte[16];

            aesGcm.Encrypt(nonce, Encoding.UTF8.GetBytes(plainText), cipherText, tag);

            //加密后的数据
            byte[] encryptedData = new byte[nonce.Length + cipherText.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, encryptedData, 0, nonce.Length);
            Buffer.BlockCopy(cipherText, 0, encryptedData, nonce.Length, cipherText.Length);
            Buffer.BlockCopy(tag, 0, encryptedData, nonce.Length + cipherText.Length, tag.Length);

            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// 解密 AES-128/GCM + BASE64
        /// </summary>
        /// <param name="base64CipherText">待解密字符串</param>
        /// <param name="base64Key"></param>
        /// <returns></returns>
        public static string DecryptByAES128AndGCM(string base64CipherText, string base64Key)
        {
            byte[] encryptedData = Convert.FromBase64String(base64CipherText);
            byte[] key = Convert.FromBase64String(base64Key);

            using AesGcm aesGcm = new(key);

            //生成随机的 12 字节 nonce
            byte[] nonce = new byte[12];
            //正文
            byte[] cipherText = new byte[encryptedData.Length - 28];
            //标签
            byte[] tag = new byte[16];

            Buffer.BlockCopy(encryptedData, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptedData, nonce.Length, cipherText, 0, cipherText.Length);
            Buffer.BlockCopy(encryptedData, nonce.Length + cipherText.Length, tag, 0, tag.Length);

            byte[] decryptedData = new byte[cipherText.Length];
            aesGcm.Decrypt(nonce, cipherText, tag, decryptedData);

            return Encoding.UTF8.GetString(decryptedData);
        }


        #endregion

        #region AES-CBC 模式（每次加密结果一样）

        /// <summary>
        /// AES-CBC 模式（每次加密结果一样）
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="base64Key"></param>
        /// <param name="ivStr">固定 16 字节 IV，注意不能变</param>
        /// <returns></returns>
        public static string EncryptDeterministicAESCBC(string plainText, string base64Key, string ivStr)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] iv = Encoding.UTF8.GetBytes(ivStr);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// AES-CBC 模式解密
        /// </summary>
        /// <param name="base64Cipher"></param>
        /// <param name="base64Key"></param>
        /// <param name="ivStr"></param>
        /// <returns></returns>
        public static string DecryptDeterministicAESCBC(string base64Cipher, string base64Key, string ivStr)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] iv = Encoding.UTF8.GetBytes(ivStr); // 和加密时一样

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(base64Cipher);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        #endregion

        #region RSA

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="originalData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string originalData, string key)
        {
            // 创建RSA实例
            using RSA rsa = RSA.Create();
            // 导入公钥
            rsa.ImportRSAPublicKey(Convert.FromBase64String(key), out _);

            // 加密字符串
            byte[] originalDataBytes = Encoding.UTF8.GetBytes(originalData);
            byte[] encryptedDataBytes = rsa.Encrypt(originalDataBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedDataBytes);
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="originalData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string RSADecrypt(string originalData, string key)
        {
            // 创建RSA实例
            using RSA rsa = RSA.Create();
            // 导入私钥
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(key), out _);

            // 加密后的数据
            byte[] encryptedDataBytes = Convert.FromBase64String(originalData);

            // 解密数据
            byte[] decryptedDataBytes = rsa.Decrypt(encryptedDataBytes, RSAEncryptionPadding.Pkcs1);
            string decryptedData = Encoding.UTF8.GetString(decryptedDataBytes);

            // 输出解密后的数据
            return decryptedData;
        }

        #endregion

    }
}
