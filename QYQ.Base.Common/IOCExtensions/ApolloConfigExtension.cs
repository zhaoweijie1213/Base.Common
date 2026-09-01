using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Enums;
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
            LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Warning);
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
        /// 【业务意图】注册一个「只取原文、不摊平」的 Apollo Namespace，供内容无法被
        /// 内置解析器处理的场景（如策划表格导出的 XML）使用。
        ///
        /// 注意：适配器按格式注册在全局静态表上，调用后该格式的**所有** Namespace 都不再
        /// 摊平。同一进程内若还有需要自动摊平的同格式 Namespace，不要使用本方法。
        /// </summary>
        /// <param name="builder">Apollo 配置构建器，由 <see cref="AddQYQApollo"/> 返回。</param>
        /// <param name="namespace">Namespace 名称，不含格式后缀（后缀由 format 拼接）。</param>
        /// <param name="sectionKey">配置键前缀，最终以 {sectionKey}:content 读取原文。</param>
        /// <param name="format">Namespace 格式，默认 Xml。</param>
        /// <returns>Apollo 配置构建器，便于链式调用。</returns>
        /// <exception cref="ArgumentException">sectionKey 为空时抛出。</exception>
        public static IApolloConfigurationBuilder AddQYQRawNamespace(this IApolloConfigurationBuilder builder, string @namespace, string sectionKey, ConfigFileFormat format = ConfigFileFormat.Xml)
        {
            // 原文统一落在 content 键上，不给前缀会污染配置根级，
            // 且多个原文 Namespace 之间会互相覆盖，因此这里强制要求 sectionKey。
            if (string.IsNullOrWhiteSpace(sectionKey))
            {
                throw new ArgumentException("sectionKey 不能为空。", nameof(sectionKey));
            }

            // 适配器替换必须发生在 IConfigurationBuilder.Build() 之前，
            // 这里先于 AddNamespace 注册，天然满足该时序要求。
            ConfigAdapterRegister.AddAdapter(format, RawContentConfigAdapter.Instance);

            return builder.AddNamespace(@namespace, sectionKey, format);
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
