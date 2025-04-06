//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;

//namespace QYQ.Base.Common.Tool
//{
//    /// <summary>
//    /// AES-GCM（Galois/Counter Mode）是一种基于 AES 的加密模式，它不仅提供“机密性”，还能同时提供“完整性校验”**，是现代加密的推荐方式。
//    /// 密文 = Nonce(随机数) + Ciphertext(密文) + Tag(认证标签)
//    /// </summary>
//    public static class AESGcmHelper
//    {
//        /// <summary>
//        /// 生成 AES 密钥（128/192/256 位），Base64 编码
//        /// </summary>
//        /// <param name="bitLength">密钥长度（支持 128、192、256）</param>
//        public static string GenerateAESKeyBase64(int bitLength = 128)
//        {
//            if (bitLength != 128 && bitLength != 192 && bitLength != 256)
//                throw new ArgumentException("只支持 128、192 或 256 位密钥");

//            byte[] key = RandomNumberGenerator.GetBytes(bitLength / 8);
//            return Convert.ToBase64String(key);
//        }

//        /// <summary>
//        /// 加密文本，返回 Base64 编码的密文
//        /// </summary>
//        public static string Encrypt(string plainText, string base64Key)
//        {
//            byte[] key = Convert.FromBase64String(base64Key);
//            //int keyBitLength = key.Length * 8;
//            byte[] nonce = RandomNumberGenerator.GetBytes(12); // GCM 推荐的 12 字节 IV
//            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
//            byte[] cipherBytes = new byte[plainBytes.Length];
//            byte[] tag = new byte[16]; // 128-bit tag

//            using var aes = new AesGcm(key, tagSizeInBytes: 128);
//            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

//            byte[] encryptedBytes = new byte[nonce.Length + cipherBytes.Length + tag.Length];
//            Buffer.BlockCopy(nonce, 0, encryptedBytes, 0, nonce.Length);
//            Buffer.BlockCopy(cipherBytes, 0, encryptedBytes, nonce.Length, cipherBytes.Length);
//            Buffer.BlockCopy(tag, 0, encryptedBytes, nonce.Length + cipherBytes.Length, tag.Length);

//            return Convert.ToBase64String(encryptedBytes);
//        }

//        /// <summary>
//        /// 解密 Base64 编码的密文
//        /// </summary>
//        public static string Decrypt(string base64CipherText, string base64Key)
//        {
//            byte[] encryptedBytes = Convert.FromBase64String(base64CipherText);
//            byte[] key = Convert.FromBase64String(base64Key);
//            //int keyBitLength = key.Length * 8;

//            byte[] nonce = new byte[12];
//            byte[] tag = new byte[16];
//            byte[] cipherBytes = new byte[encryptedBytes.Length - nonce.Length - tag.Length];

//            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, nonce.Length);
//            Buffer.BlockCopy(encryptedBytes, nonce.Length, cipherBytes, 0, cipherBytes.Length);
//            Buffer.BlockCopy(encryptedBytes, nonce.Length + cipherBytes.Length, tag, 0, tag.Length);

//            byte[] plainBytes = new byte[cipherBytes.Length];

//            try
//            {
//                using var aes = new AesGcm(key, tagSizeInBytes: 128);
//                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
//                return Encoding.UTF8.GetString(plainBytes);
//            }
//            catch (CryptographicException ex)
//            {
//                throw new InvalidOperationException("解密失败，数据可能被篡改或密钥错误", ex);
//            }
//        }
//    }
//}
