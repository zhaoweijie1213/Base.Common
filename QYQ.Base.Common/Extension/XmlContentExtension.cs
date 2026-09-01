using Com.Ctrip.Framework.Apollo.Core;
using Microsoft.Extensions.Configuration;
using QYQ.Base.Common.Xml;
using System;
using System.Collections.Generic;

namespace QYQ.Base.Common.Extension
{
    /// <summary>
    /// 【用途】XML 原文映射扩展方法。
    ///
    /// 配合 AddQYQRawNamespace 使用：后者把无法被内置解析器摊平的 Namespace 原样取回，
    /// 本类负责把那段原文按 <see cref="XmlAttributeNameAttribute"/> 的声明转成业务对象。
    /// </summary>
    public static class XmlContentExtension
    {
        /// <summary>
        /// 【业务意图】把一段 XML 原文解析为 <typeparamref name="T"/> 的集合。
        ///
        /// 只映射 XML 节点上的属性（attribute），不读取子元素文本；
        /// 目标类型上未标注 <see cref="XmlAttributeNameAttribute"/> 的属性不参与映射。
        /// </summary>
        /// <typeparam name="T">目标类型，需要有公开无参构造。</typeparam>
        /// <param name="content">XML 原文；为空视为配置尚未下发，返回空集合。</param>
        /// <param name="elementName">只取该名字的子节点；为 null 时取根节点下全部子节点。</param>
        /// <returns>解析出的对象集合，顺序与原文中的节点顺序一致。</returns>
        /// <exception cref="XmlContentParseException">原文畸形、必填属性缺失、值无法转换或目标类型不受支持。</exception>
        public static IReadOnlyList<T> ParseXmlItems<T>(this string content, string? elementName = null) where T : new()
            => XmlContentMapper.Parse<T>(content, elementName);

        /// <summary>
        /// 【业务意图】从配置的 {sectionKey}:content 取出 XML 原文并解析为 <typeparamref name="T"/> 的集合。
        ///
        /// sectionKey 与 AddQYQRawNamespace 注册时传入的保持一致即可。
        /// </summary>
        /// <typeparam name="T">目标类型，需要有公开无参构造。</typeparam>
        /// <param name="configuration">配置对象。</param>
        /// <param name="sectionKey">注册原文 Namespace 时使用的配置键前缀。</param>
        /// <param name="elementName">只取该名字的子节点；为 null 时取根节点下全部子节点。</param>
        /// <returns>解析出的对象集合；配置里没有该键时返回空集合。</returns>
        /// <exception cref="ArgumentException">sectionKey 为空时抛出。</exception>
        /// <exception cref="XmlContentParseException">原文畸形、必填属性缺失、值无法转换或目标类型不受支持。</exception>
        public static IReadOnlyList<T> GetXmlItems<T>(this IConfiguration configuration, string sectionKey, string? elementName = null) where T : new()
        {
            ArgumentNullException.ThrowIfNull(configuration);

            if (string.IsNullOrWhiteSpace(sectionKey))
            {
                throw new ArgumentException("sectionKey 不能为空。", nameof(sectionKey));
            }

            // 复用 Apollo 的 content 常量，与写入端 RawContentConfigAdapter 保持同一个真相来源
            var content = configuration[$"{sectionKey}:{ConfigConsts.ConfigFileContentKey}"];

            return XmlContentMapper.Parse<T>(content, elementName);
        }
    }
}
