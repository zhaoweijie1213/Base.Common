using NJsonSchema;
using NJsonSchema.Generation;
using System.Collections.Concurrent;
using System.ComponentModel;
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
                    string description = string.Join("<br/>", items.Select(f => $"{f.Item1}:{f.Item2},{f.Item3};{Environment.NewLine}"));
                    schema.Description = string.IsNullOrEmpty(schema.Description) ? description : $"{schema.Description}:<br/><br/>{description}";
                }
            }
            else if (context.ContextualType.OriginalType.IsClass && context.ContextualType.OriginalType != typeof(string))
            {
                UpdateSchemaDescription(schema);
            }
        }

        private void UpdateSchemaDescription(JsonSchema schema)
        {
            if (schema.HasReference)
            {
                var s = schema.ActualSchema;
                if (s != null && s.Enumeration != null && s.Enumeration.Count > 0)
                {
                    if (!string.IsNullOrEmpty(s.Description))
                    {
                        string description = $"【{s.Description}】";
                        if (string.IsNullOrEmpty(schema.Description) || !schema.Description.EndsWith(description))
                        {
                            schema.Description += description;
                        }
                    }
                }
            }

            foreach (var key in schema.Properties.Keys)
            {
                var s = schema.Properties[key];
                UpdateSchemaDescription(s);
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
                    var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    string key = BuildEnumDescription(attribute?.Description, _xmlCommentProvider.GetSummary(field), field.Name);
                    int value = 0;
                    var enumValue = enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);
                    if (enumValue != null)
                    {
                        value = (int)enumValue;
                    }

                    tuples.Add(new Tuple<object, string, string>(value, field.Name, key));
                }
            }
            _dict.TryAdd(enumType, tuples);
            return tuples;
        }

        private static string BuildEnumDescription(string? attributeDescription, string xmlSummary, string fieldName)
        {
            var description = string.IsNullOrWhiteSpace(attributeDescription) ? string.Empty : attributeDescription.Trim();
            var summary = string.IsNullOrWhiteSpace(xmlSummary) ? string.Empty : xmlSummary.Trim();

            if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(summary) && !string.Equals(description, summary, StringComparison.Ordinal))
            {
                return $"{description}（{summary}）";
            }

            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            return string.IsNullOrEmpty(summary) ? fieldName : summary;
        }
    }
}
