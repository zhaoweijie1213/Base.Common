using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Extension
{
    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LoggerExtension
    {
        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="method"></param>
        /// <param name="e"></param>
        public static void BaseErrorLog(this ILogger logger,string method,Exception e)
        {
            logger.LogError($"{method}:{e.Message}\n{e.StackTrace}");
        }
    }
}
