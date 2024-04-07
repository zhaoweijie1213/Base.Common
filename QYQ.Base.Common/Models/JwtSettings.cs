using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Models
{
    /// <summary>
    /// jwt设置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        ///  token是谁颁发的
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// token可以给那些客户端使用
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 加密的key（SecretKey必须大于16个,是大于，不是大于等于） 
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 刷新令牌过期时间
        /// </summary>
        public int RefreshExpiration { get; set; }

        /// <summary>
        /// 访问令牌过期时间
        /// </summary>
        public int AccessExpiration { get; set; }
    }
}
