using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// 日志注册扩展
    /// </summary>
    public static class SerilogExtension
    {
        /// <summary>
        /// 在 HostApplicationBuilder 中添加 QYQ Serilog 配置
        /// </summary>
        /// <param name="builder">主机构建器</param>
        /// <param name="preserveStaticLogger">是否保留静态日志记录器</param>
        /// <param name="writeToProviders">是否写入日志提供者</param>
        /// <returns></returns>
        public static HostApplicationBuilder AddQYQSerilog(this HostApplicationBuilder builder, bool preserveStaticLogger = true, bool writeToProviders = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddQYQSerilog(builder.Configuration, preserveStaticLogger, writeToProviders);

            return builder;
        }


        /// <summary>
        /// 在 WebApplicationBuilder 中添加 QYQ Serilog 配置
        /// </summary>
        /// <param name="builder">主机构建器</param>
        /// <param name="preserveStaticLogger">是否保留静态日志记录器</param>
        /// <param name="writeToProviders">是否写入日志提供者</param>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQSerilog(this WebApplicationBuilder builder, bool preserveStaticLogger = true, bool writeToProviders = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddQYQSerilog(builder.Configuration, preserveStaticLogger, writeToProviders);

            return builder;
        }

        /// <summary>
        /// 在 IServiceCollection 中添加 QYQ Serilog 配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="preserveStaticLogger">是否保留静态日志记录器</param>
        /// <param name="writeToProviders">是否写入日志提供者</param>
        /// <returns></returns>
        public static IServiceCollection AddQYQSerilog(this IServiceCollection services, IConfiguration configuration, bool preserveStaticLogger = true, bool writeToProviders = true)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            services.AddSerilog((context, configureLogger) =>
            {
                configureLogger
                    .ReadFrom.Configuration(configuration)
                    .Enrich.WithMachineName()
                    .Enrich.FromLogContext();
            }, preserveStaticLogger, writeToProviders);

            return services;
        }

    }
}
