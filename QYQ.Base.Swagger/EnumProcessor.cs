using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    public class EnumProcessor : ISchemaProcessor
    {
        static readonly ConcurrentDictionary<Type, List<Tuple<object, string, string>>> dict = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Process(SchemaProcessorContext context)
        {
            var schema = context.Schema;
            if (context.ContextualType.OriginalType.IsEnum)
            {
                var items = GetTextValueItems(context.ContextualType.OriginalType);
                if (items.Count > 0)
                {
                    //枚举描述
                    string decription = string.Join("<br/>", items.Select(f => $"{f.Item1}:{f.Item2},{f.Item3};{Environment.NewLine}"));
                    schema.Description = string.IsNullOrEmpty(schema.Description) ? decription : $"{schema.Description}:<br/><br/>{decription}";
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
        /// 获取枚 举值+名称+描述  
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        private List<Tuple<object, string, string>> GetTextValueItems(Type enumType)
        {
            if (dict.TryGetValue(enumType, out List<Tuple<object, string, string>>? tuples) && tuples != null)
            {
                return tuples;
            }
            tuples = new();
            FieldInfo[] fields = enumType.GetFields();
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }
                    string key = attribute?.Description ?? field.Name;
                    int value = 0;
                    var enumValue = enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);
                    if (enumValue != null)
                    {
                        value = (int)enumValue;
                    }
                    
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    tuples.Add(new Tuple<object, string, string>(value, field.Name, key));
                    //list.Add(new KeyValuePair<string, int>(key, value));
                }
            }
            dict.TryAdd(enumType, tuples);
            return tuples;
        }
    }
}
