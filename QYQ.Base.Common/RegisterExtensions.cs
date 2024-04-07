using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using QYQ.Base.Common.ApiResult;
using QYQ.Base.Common.Middleware;
using QYQ.Base.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common
{
    /// <summary>
    /// 注册
    /// </summary>
    public static class RegisterExtensions
    {
        /// <summary>
        /// api返回结果处理
        /// </summary>
        /// <param name="services"></param>
        public static void AddBaseCommonApiResult(this IServiceCollection services)
        {
            services.AddMvc(options => {
                //加入返回结果处理
                options.Filters.Add<ApiResultFilter>();

            });
        }

        /// <summary>
        /// 添加Http日志的配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddQYQHttpLogging(this IServiceCollection services, Action<QYQHttpLoggingOptions>? action = null)
        {
            if (action == null)
            {
                action = options =>
                {
                    options.IgnorePath = DefaultOptionsHelpers.DefaultIgnorePath;
                };
            }
            services.Configure(action);
            return services;
        }

    }
}
