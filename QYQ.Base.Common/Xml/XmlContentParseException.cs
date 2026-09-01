using System;

namespace QYQ.Base.Common.Xml
{
    /// <summary>
    /// 【用途】XML 原文映射为对象时的统一失败异常。
    ///
    /// XML 本身畸形、必填属性缺失、属性值类型转换失败、目标类型不受支持，
    /// 全部收敛到本异常类型，调用方只需 catch 一种即可沿用上一份可用快照。
    /// </summary>
    /// <param name="message">中文错误说明，已包含定位信息。</param>
    /// <param name="itemIndex">出错节点在集合中的序号（从 0 开始）；无法定位到具体节点时为 -1。</param>
    /// <param name="lineNumber">出错位置在原文中的行号；取不到行信息时为 0。</param>
    /// <param name="linePosition">出错位置在原文中的列号；取不到行信息时为 0。</param>
    /// <param name="attributeName">出错的 XML 属性名；与具体属性无关时为 null。</param>
    /// <param name="rawValue">出错属性的原始文本值；与具体值无关时为 null。</param>
    /// <param name="targetType">期望转换成的目标类型；与类型转换无关时为 null。</param>
    /// <param name="innerException">底层异常，便于排查。</param>
    public sealed class XmlContentParseException(
        string message,
        int itemIndex = -1,
        int lineNumber = 0,
        int linePosition = 0,
        string? attributeName = null,
        string? rawValue = null,
        Type? targetType = null,
        Exception? innerException = null) : Exception(message, innerException)
    {
        /// <summary>
        /// 【含义】出错节点在集合中的序号（从 0 开始）；无法定位到具体节点时为 -1。
        /// </summary>
        public int ItemIndex { get; } = itemIndex;

        /// <summary>
        /// 【含义】出错位置在原文中的行号；取不到行信息时为 0。
        /// </summary>
        public int LineNumber { get; } = lineNumber;

        /// <summary>
        /// 【含义】出错位置在原文中的列号；取不到行信息时为 0。
        /// </summary>
        public int LinePosition { get; } = linePosition;

        /// <summary>
        /// 【含义】出错的 XML 属性名；与具体属性无关时为 null。
        /// </summary>
        public string? AttributeName { get; } = attributeName;

        /// <summary>
        /// 【含义】出错属性的原始文本值；与具体值无关时为 null。
        /// </summary>
        public string? RawValue { get; } = rawValue;

        /// <summary>
        /// 【含义】期望转换成的目标类型；与类型转换无关时为 null。
        /// </summary>
        public Type? TargetType { get; } = targetType;
    }
}
