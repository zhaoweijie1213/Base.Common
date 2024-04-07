//using Microsoft.OpenApi.Any;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace QYQ.Base.Swagger
//{
//    /// <summary>
//    /// swagger 枚举映射，
//    /// 原因：前端代理生成枚举是数字
//    /// </summary>
//    public class EnumSchemaFilter : ISchemaFilter
//    {

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="schema"></param>
//        /// <param name="context"></param>
//        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
//        {
//            if (context.Type.IsEnum)
//            {
//                List<OpenApiInteger> list = new();
//                foreach (var val in schema.Enum)
//                {
//                    list.Add((OpenApiInteger)val);
//                }

//                schema.Description += DescribeEnum(context.Type, list);
//            }

//        }


//        private static string DescribeEnum(Type type, List<OpenApiInteger> enums)
//        {
//            var enumDescriptions = new List<string>();
//            foreach (var item in enums)
//            {
//                if (type == null) continue;
//                var value = Enum.Parse(type, item.Value.ToString());
//                var desc = GetDescription(type, value);

//                if (string.IsNullOrEmpty(desc))
//                    enumDescriptions.Add($"{item.Value}:{Enum.GetName(type, value)}; ");
//                else
//                    enumDescriptions.Add($"{item.Value}:{Enum.GetName(type, value)},{desc}; ");

//            }
//            return $"<br/>{Environment.NewLine}{string.Join("<br/>" + Environment.NewLine, enumDescriptions)}";
//        }

//        /// <summary>
//        /// 获取枚举描述
//        /// </summary>
//        /// <param name="t"></param>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        private static string GetDescription(Type t, object value)
//        {
//            foreach (MemberInfo mInfo in t.GetMembers())
//            {
//                if (mInfo.Name == t.GetEnumName(value))
//                {
//                    foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
//                    {
//                        if (attr.GetType() == typeof(DescriptionAttribute))
//                        {
//                            return ((DescriptionAttribute)attr).Description;
//                        }
//                    }
//                }
//            }
//            return string.Empty;
//        }

//    }
//}
