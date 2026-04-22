using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// apollo 扩展
    /// </summary>
    public static class ApolloConfigExtension
    {

        /// <summary>
        /// 添加apollo配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApolloConfigurationBuilder AddQYQApollo(this IConfigurationBuilder builder)
        {
            LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
            var configuration = builder.Build();
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "配置不能为空");
            }
            //获取apollo配置
            ApolloOptions? apollo = configuration.GetSection("apollo").Get<ApolloOptions>() ?? throw new InvalidOperationException("Apollo 配置缺失或无效");
            //var cluster = configuration.GetSection("apollo:Cluster").Get<string>();
            ////读取集群
            //if (!string.IsNullOrEmpty(cluster))
            //{
            //    apollo.Cluster = cluster;
            //}
            //设置apollo的服务地址
            List<string> configServer = new()
            {
                apollo.Meta[apollo.Env.ToString()]
            };
            apollo.ConfigServer = configServer;
            return builder.AddApollo(apollo);
        }

        /// <summary>
        /// 启用 Apollo 日志
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static WebApplication UseQYQApolloLogger(this WebApplication app)
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            MelLogging.UseMel(loggerFactory);
            return app;
        }

        /// <summary>
        /// 启用 Apollo 日志
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IHost UseQYQApolloLogger(this IHost app)
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            MelLogging.UseMel(loggerFactory);
            return app;
        }
    }
}
