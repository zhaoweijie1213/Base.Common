using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace QYQ.Base.Swagger
{
    /// <summary>
    /// 从运行目录 XML 文档中读取类型和成员注释。
    /// </summary>
    internal sealed class XmlCommentProvider : IXmlCommentProvider
    {
        private readonly ConcurrentDictionary<string, string> _summaries;

        /// <summary>
        /// 使用成员注释字典创建 XML 注释读取器。
        /// </summary>
        /// <param name="summaries">XML 成员名与 Summary 注释的映射。</param>
        internal XmlCommentProvider(IDictionary<string, string> summaries)
        {
            _summaries = new ConcurrentDictionary<string, string>(summaries);
        }

        /// <summary>
        /// 从当前应用运行目录加载全部 XML 注释文件。
        /// </summary>
        /// <returns>包含已加载 XML Summary 注释的读取器。</returns>
        public static XmlCommentProvider FromAppContextBaseDirectory()
        {
            return FromDirectory(AppContext.BaseDirectory);
        }

        /// <summary>
        /// 从指定目录加载全部 XML 注释文件。
        /// </summary>
        /// <param name="directory">XML 注释文件所在目录。</param>
        /// <returns>包含已加载 XML Summary 注释的读取器。</returns>
        internal static XmlCommentProvider FromDirectory(string directory)
        {
            var summaries = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return new XmlCommentProvider(summaries);
            }

            foreach (var xmlFile in Directory.GetFiles(directory, "*.xml"))
            {
                LoadXmlFile(xmlFile, summaries);
            }

            return new XmlCommentProvider(summaries);
        }

        /// <summary>
        /// 获取类型的 XML Summary 注释。
        /// </summary>
        /// <param name="type">需要读取注释的类型。</param>
        /// <returns>存在时返回注释内容，否则返回空字符串。</returns>
        public string GetSummary(Type type)
        {
            return GetSummary($"T:{GetXmlTypeName(type)}");
        }

        /// <summary>
        /// 获取字段的 XML Summary 注释。
        /// </summary>
        /// <param name="fieldInfo">需要读取注释的字段。</param>
        /// <returns>存在时返回注释内容，否则返回空字符串。</returns>
        public string GetSummary(FieldInfo fieldInfo)
        {
            return fieldInfo.DeclaringType == null
                ? string.Empty
                : GetSummary($"F:{GetXmlTypeName(fieldInfo.DeclaringType)}.{fieldInfo.Name}");
        }

        private string GetSummary(string memberName)
        {
            return _summaries.TryGetValue(memberName, out var summary) ? summary : string.Empty;
        }

        private static void LoadXmlFile(string xmlFile, Dictionary<string, string> summaries)
        {
            try
            {
                var document = XDocument.Load(xmlFile);
                var members = document.Root?.Element("members")?.Elements("member") ?? Enumerable.Empty<XElement>();
                foreach (var member in members)
                {
                    var name = member.Attribute("name")?.Value;
                    var summary = NormalizeSummary(member.Element("summary")?.Value);
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(summary))
                    {
                        summaries[name] = summary;
                    }
                }
            }
            catch
            {
                // XML 注释缺失或格式异常时不应影响 Swagger 文档生成。
            }
        }

        private static string NormalizeSummary(string? summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            var lines = summary
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));

            return string.Join(" ", lines);
        }

        private static string GetXmlTypeName(Type type)
        {
            return (type.FullName ?? type.Name).Replace('+', '.');
        }
    }
}
