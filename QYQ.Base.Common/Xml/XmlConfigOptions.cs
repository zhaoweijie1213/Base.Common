using System.Collections.Generic;

namespace QYQ.Base.Common.Xml
{
    /// <summary>
    /// 【用途】承载从 XML 原文解析出的条目集合的配置选项。
    ///
    /// 由 AddQYQXmlOptions 注册，通过 IOptionsMonitor&lt;XmlConfigOptions&lt;T&gt;&gt; 读取；
    /// 使用方无需再为每个配置单独定义一个 Options 类。
    /// </summary>
    /// <typeparam name="T">条目类型，属性上用 <see cref="XmlAttributeNameAttribute"/> 声明映射关系。</typeparam>
    public class XmlConfigOptions<T> where T : new()
    {
        /// <summary>
        /// 【含义】解析出的条目集合，顺序与 XML 原文中的节点顺序一致。
        /// </summary>
        public IReadOnlyList<T> Items { get; set; } = [];
    }
}
