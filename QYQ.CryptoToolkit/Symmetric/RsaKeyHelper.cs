using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QYQ.CryptoToolkit.Symmetric
{
    /// <summary>
    /// RSA 密钥帮助类
    /// </summary>
    public static class RsaKeyHelper
    {
        /// <summary>
        /// 文件中读取 Base64 编码的密钥
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string GetKeyFromTextFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件未找到", filePath);

            return File.ReadAllText(filePath).Trim();
        }

        /// <summary>
        /// 从 PEM 文件中获取 Base64 编码的密钥
        /// </summary>
        /// <param name="pemFilePath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string GetKeyFromPemFile(string pemFilePath)
        {
            if (!File.Exists(pemFilePath))
                throw new FileNotFoundException("PEM 文件未找到", pemFilePath);

            var pemContent = File.ReadAllText(pemFilePath);
            var base64 = ExtractPemBody(pemContent);

            return base64;
        }

        /// <summary>
        /// 从 PEM 字符串中提取 Base64 主体部分
        /// </summary>
        public static string ExtractPemBody(string pem)
        {
            var lines = pem.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("-----"))
                {
                    sb.Append(trimmed);
                }
            }
            return sb.ToString();
        }

    }
}
