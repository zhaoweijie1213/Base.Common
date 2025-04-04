using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Tool
{
    /// <summary>
    /// RSA 解密工具类
    /// </summary>
    public static class RsaDecryptHelper
    {
        /// <summary>
        /// 从 txt 文件中读取私钥（如 rsa_private_key_base64.txt）
        /// </summary>
        public static string GetBase64KeyFromTextFile(string base64FilePath)
        {
            if (!File.Exists(base64FilePath))
                throw new FileNotFoundException("文件未找到", base64FilePath);

            return File.ReadAllText(base64FilePath).Trim();
        }

        /// <summary>
        /// .pem 文件获取私钥
        /// </summary>
        public static string GetBase64KeyWithPemFile(string pemFilePath)
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
            var lines = pem.Split('\n');
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
