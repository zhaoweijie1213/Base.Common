using NJsonSchema;
using NJsonSchema.Generation;
using System.Collections.Concurrent;
using System.Reflection;

namespace QYQ.Base.Swagger
{
    /// <summary>
    /// 为 Swagger Schema 补充枚举值说明。
    /// </summary>
    public class EnumProcessor : ISchemaProcessor
    {
        private static readonly Lazy<XmlCommentProvider> DefaultXmlCommentProvider = new(XmlCommentProvider.FromAppContextBaseDirectory);
        private readonly IXmlCommentProvider _xmlCommentProvider;
        private readonly ConcurrentDictionary<Type, List<Tuple<object, string, string>>> _dict = new();

        /// <summary>
        /// 创建枚举 Schema 处理器。
        /// </summary>
        public EnumProcessor() : this(DefaultXmlCommentProvider.Value)
        {
        }

        /// <summary>
        /// 创建枚举 Schema 处理器。
        /// </summary>
        /// <param name="xmlCommentProvider">XML 注释读取器。</param>
        internal EnumProcessor(IXmlCommentProvider xmlCommentProvider)
        {
            _xmlCommentProvider = xmlCommentProvider;
        }

        /// <summary>
        /// 为枚举 Schema 和引用枚举的属性补充说明。
        /// </summary>
        /// <param name="context">Schema 处理上下文。</param>
        public void Process(SchemaProcessorContext context)
        {
            var schema = context.Schema;
            if (context.ContextualType.OriginalType.IsEnum)
            {
                var items = GetTextValueItems(context.ContextualType.OriginalType);
                if (items.Count > 0)
                {
                    // 枚举描述
                    string description = string.Join("<br/>", items.Select(f => $"{f.Item3};{Environment.NewLine}"));
                    schema.Description = string.IsNullOrEmpty(schema.Description) ? description : $"{schema.Description}:<br/><br/>{description}";
                }
            }
        }

        /// <summary>
        /// 获取枚举值、名称和说明。
        /// </summary>
        /// <param name="enumType">枚举类型。</param>
        /// <returns>枚举值、名称和说明集合。</returns>
        private List<Tuple<object, string, string>> GetTextValueItems(Type enumType)
        {
            if (_dict.TryGetValue(enumType, out List<Tuple<object, string, string>>? tuples) && tuples != null)
            {
                return tuples;
            }

            tuples = new();
            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    int value = 0;
                    var enumValue = enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);
                    if (enumValue != null)
                    {
                        value = (int)enumValue;
                    }

                    var summary = _xmlCommentProvider.GetSummary(field);
                    tuples.Add(new Tuple<object, string, string>(value, field.Name, BuildEnumDescription(field.Name, value, summary)));
                }
            }
            _dict.TryAdd(enumType, tuples);
            return tuples;
        }

        private static string BuildEnumDescription(string fieldName, int value, string xmlSummary)
        {
            var summary = string.IsNullOrWhiteSpace(xmlSummary) ? string.Empty : xmlSummary.Trim();
            return string.IsNullOrEmpty(summary) ? $"{fieldName} = {value}" : $"{fieldName} = {value}（{summary}）";
        }
    }
}
