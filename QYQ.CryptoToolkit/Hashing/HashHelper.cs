using System;
using System.Security.Cryptography;
using System.Text;

namespace QYQ.CryptoToolkit.Hashing
{
    /// <summary>
    /// hash算法
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// HMAC SHA256 (Base64)
        /// </summary>
        public static string HmacSha256Base64(string message, string secret)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            var encoding = Encoding.ASCII;
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                Span<byte> hashBytes = stackalloc byte[32]; // HMACSHA256 输出长度固定为 32 字节
                hmacsha256.TryComputeHash(messageBytes, hashBytes, out _);
                return Convert.ToBase64String(hashBytes);
#else
                byte[] hashBytes = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashBytes);
#endif
            }
        }

        /// <summary>
        /// HMAC SHA256 (Hex 小写字符串)
        /// </summary>
        public static string HmacSha256(string message, string secret)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            var encoding = Encoding.UTF8;
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                Span<byte> hashBytes = stackalloc byte[32]; // 256 位 = 32 字节
                hmacsha256.TryComputeHash(messageBytes, hashBytes, out _);

                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    builder.AppendFormat("{0:x2}", b);
                }
                return builder.ToString();
#else
                byte[] hashBytes = hmacsha256.ComputeHash(messageBytes);
                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    builder.AppendFormat("{0:x2}", b);
                }
                return builder.ToString();
#endif
            }
        }

        /// <summary>
        /// MD5 字符串加密（小写）
        /// </summary>
        public static string Md5Hash(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            using (MD5 md5 = MD5.Create())
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                Span<byte> hashBytes = stackalloc byte[16]; // MD5 输出固定长度 16 字节
                md5.TryComputeHash(inputBytes, hashBytes, out _);

                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    builder.AppendFormat("{0:x2}", b);
                }
                return builder.ToString();
#else
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    builder.AppendFormat("{0:x2}", b);
                }
                return builder.ToString();
#endif
            }
        }

        /// <summary>
        /// string 转 hex 字符串
        /// </summary>
        public static string ToHexString(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            byte[] bytes = Encoding.Unicode.GetBytes(input);
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// hex 字符串 转 string
        /// </summary>
        public static string FromHexString(string hexString)
        {
            if (hexString == null) throw new ArgumentNullException(nameof(hexString));
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even length.", nameof(hexString));
            }

            int byteCount = hexString.Length / 2;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            Span<byte> bytes = byteCount <= 128 ? stackalloc byte[byteCount] : new byte[byteCount];
#else
            byte[] bytes = new byte[byteCount];
#endif
            for (int i = 0; i < byteCount; i++)
            {
                string byteValue = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteValue, 16);
            }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            return Encoding.Unicode.GetString(bytes);
#else
            return Encoding.Unicode.GetString(bytes);
#endif
        }
    }
}
