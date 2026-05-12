using System.Reflection;

namespace QYQ.Base.Swagger
{
    /// <summary>
    /// 提供 XML 注释摘要读取能力。
    /// </summary>
    internal interface IXmlCommentProvider
    {
        /// <summary>
        /// 获取类型的 XML Summary 注释。
        /// </summary>
        /// <param name="type">需要读取注释的类型。</param>
        /// <returns>存在时返回注释内容，否则返回空字符串。</returns>
        string GetSummary(Type type);

        /// <summary>
        /// 获取字段的 XML Summary 注释。
        /// </summary>
        /// <param name="fieldInfo">需要读取注释的字段。</param>
        /// <returns>存在时返回注释内容，否则返回空字符串。</returns>
        string GetSummary(FieldInfo fieldInfo);
    }
}
