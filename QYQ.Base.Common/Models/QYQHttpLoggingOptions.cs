using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class QYQHttpLoggingOptions
    {
        /// <summary>
        /// 忽略的路径
        /// </summary>
        public List<string> IgnorePath { get; set; } = [];

        /// <summary>
        /// 最大请求体,默认限制为1MB
        /// </summary>
        public int MaxRequestBodySize { get; set; } = 1024 * 1024;

        /// <summary>
        /// 最大响应,默认限制为1MB
        /// </summary>
        public int MaxResponseLenght { get; set; } = 1024 * 1024;

        /// <summary>
        /// 长查询阈值(毫秒)
        /// </summary>
        public int LongQueryThreshold { get; set; } = 100;

    }
}
