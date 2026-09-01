using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Core.Utils;
using System.Collections.Generic;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// 【用途】让指定格式的 Apollo Namespace 原样保留文件内容，不做键值摊平。
    ///
    /// 客户端内置的 XmlConfigAdapter 按微软 AddXmlFile 的规则摊平 XML：同名兄弟节点
    /// 只要没有 Name 属性就会撞键并抛 FormatException。由策划表格导出的配置天然是
    /// 同名节点重复、且可能带中文属性名，这类内容无法交给内置解析器。
    /// 本适配器把原文原样放在 content 键上，由业务侧自行解析。
    /// </summary>
    public sealed class RawContentConfigAdapter : ContentConfigAdapter
    {
        /// <summary>
        /// 【含义】无状态适配器，全局复用同一实例。
        /// </summary>
        public static RawContentConfigAdapter Instance { get; } = new();

        /// <summary>
        /// 私有构造，强制通过 <see cref="Instance"/> 获取实例。
        /// </summary>
        private RawContentConfigAdapter() { }

        /// <summary>
        /// 【业务意图】把 Namespace 的整段原文放回 content 键，不做任何解析。
        /// </summary>
        /// <param name="content">Namespace 的原始文本内容，可能为 null（服务端未下发内容）。</param>
        /// <returns>仅含 content 一项的配置集合；原文缺失时该项为空字符串。</returns>
        public override Properties GetProperties(string content) =>
            new(new Dictionary<string, string>(1) { [ConfigConsts.ConfigFileContentKey] = content ?? string.Empty });
    }
}
