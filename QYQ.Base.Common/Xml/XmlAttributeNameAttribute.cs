using System;

namespace QYQ.Base.Common.Xml
{
    /// <summary>
    /// 【用途】标记 DTO 属性对应 XML 节点上的哪一个属性（attribute）名。
    ///
    /// 策划表格导出的配置常带中文属性名（如 游戏ID），且属性名与 C# 命名规范无法对齐，
    /// 因此由本特性显式声明映射关系；未标注本特性的属性不参与映射，也不做任何校验。
    ///
    /// 命名上刻意避开 BCL 的 System.Xml.Serialization.XmlAttributeAttribute，
    /// 防止两者同时被 using 时静默混淆。
    /// </summary>
    /// <param name="name">对应 XML 节点上的属性名。</param>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class XmlAttributeNameAttribute(string name) : Attribute
    {
        /// <summary>
        /// 【含义】对应 XML 节点上的属性名，允许中文。
        /// </summary>
        public string Name { get; } = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("XML 属性名不能为空。", nameof(name))
            : name;

        /// <summary>
        /// 【含义】该属性是否必须出现在 XML 节点上。
        ///
        /// true（默认）：节点上缺少该属性即视为配置写错，直接抛出异常；
        /// false：缺少时保留 CLR 默认值（可空类型为 null）。
        ///
        /// 注意本项只管「缺不缺」。属性存在但值转换失败属于写错了值，无论本项如何都会抛。
        /// </summary>
        public bool Required { get; set; } = true;
    }
}
