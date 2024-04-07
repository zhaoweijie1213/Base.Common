using Microsoft.AspNetCore.Builder;
using QYQ.Base.Common.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// 添加http请求日志中间件
        /// </summary>
        /// <param name="app"></param>
        public static void UseQYQHttpLogging(this WebApplication app)
        {
            app.UseMiddleware<HttpLoggingMiddleware>();
        }
    }
}
