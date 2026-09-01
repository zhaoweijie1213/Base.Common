using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QYQ.Base.Common.Extension;
using QYQ.Base.Common.Xml;
using System;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// 【用途】把 Apollo 原文 Namespace 里的 XML 接成可热更新的配置选项。
    /// </summary>
    public static class XmlConfigOptionsExtension
    {
        /// <summary>
        /// 【业务意图】注册 <see cref="XmlConfigOptions{T}"/>，让 {sectionKey}:content 上的 XML 原文
        /// 被解析成 <typeparamref name="T"/> 集合，并在 Apollo 推送新内容时自动重算。
        ///
        /// 之所以把这两步收进库里：<c>AddOptions().Configure(委托)</c> 不像 <c>Bind()</c> 那样自带
        /// 配置变更令牌，少注册一个 <see cref="IOptionsChangeTokenSource{TOptions}"/>，
        /// <see cref="IOptionsMonitor{TOptions}"/> 的缓存就永不失效——编译通过、运行不报错，
        /// 只是热更新静默失效，各自手写极易漏。
        ///
        /// 注意：读取端必须用 <see cref="IOptionsMonitor{TOptions}"/> 或 <see cref="IOptionsSnapshot{TOptions}"/>；
        /// <see cref="IOptions{TOptions}"/> 是单例快照，拿不到新值。
        ///
        /// 同一个 <typeparamref name="T"/> 只应注册一次；重复注册不同 sectionKey 时后者会覆盖前者。
        ///
        /// 本方法不做容错：原文畸形时 <c>CurrentValue</c> 会抛 <see cref="XmlContentParseException"/>，
        /// 且每次访问都抛。需要「解析失败沿用旧快照」或「条数为 0 视为故障」的场景，
        /// 请在业务侧自建单例目录服务并订阅 <c>IConfiguration.GetReloadToken</c>，
        /// 那类判断属于业务决策，不进通用库。
        /// </summary>
        /// <typeparam name="T">条目类型，需要有公开无参构造。</typeparam>
        /// <param name="services">服务集合。</param>
        /// <param name="sectionKey">注册原文 Namespace 时使用的配置键前缀，与 AddQYQRawNamespace 传入的一致。</param>
        /// <param name="elementName">只取该名字的子节点；为 null 时取根节点下全部子节点。</param>
        /// <returns>服务集合，便于链式调用。</returns>
        /// <exception cref="ArgumentException">sectionKey 为空时抛出。</exception>
        public static IServiceCollection AddQYQXmlOptions<T>(this IServiceCollection services, string sectionKey, string? elementName = null) where T : new()
        {
            ArgumentNullException.ThrowIfNull(services);

            if (string.IsNullOrWhiteSpace(sectionKey))
            {
                throw new ArgumentException("sectionKey 不能为空。", nameof(sectionKey));
            }

            // 原文是单个字符串键，Bind 绑不出集合，只能走 Configure 委托跑解析
            services.AddOptions<XmlConfigOptions<T>>()
                .Configure<IConfiguration>((options, configuration) =>
                    options.Items = configuration.GetXmlItems<T>(sectionKey, elementName));

            // 补上 Configure 委托缺失的变更令牌，Apollo 推送触发配置重载后 Options 才会重算
            services.AddSingleton<IOptionsChangeTokenSource<XmlConfigOptions<T>>>(
                sp => new ConfigurationChangeTokenSource<XmlConfigOptions<T>>(sp.GetRequiredService<IConfiguration>()));

            return services;
        }
    }
}
