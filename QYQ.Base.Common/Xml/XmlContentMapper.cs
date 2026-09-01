using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace QYQ.Base.Common.Xml
{
    /// <summary>
    /// 【用途】把 XML 原文按 <see cref="XmlAttributeNameAttribute"/> 声明的映射关系转成对象集合。
    ///
    /// 只对外暴露扩展方法（QYQ.Base.Common.Extension.XmlContentExtension），
    /// 本类保持 internal，避免同一能力出现两套等价的公开入口。
    /// </summary>
    internal static class XmlContentMapper
    {
        /// <summary>
        /// 【含义】类型 → 映射关系的缓存，避免每次解析都重新走一遍反射。
        /// </summary>
        private static readonly ConcurrentDictionary<Type, XmlPropertyMapping[]> _mappingCache = new();

        /// <summary>
        /// 【业务意图】把 XML 原文解析为 <typeparamref name="T"/> 的集合。
        /// </summary>
        /// <typeparam name="T">目标类型，需要有公开无参构造。</typeparam>
        /// <param name="content">XML 原文；为空视为「配置尚未下发」，返回空集合。</param>
        /// <param name="elementName">只取该名字的子节点；为 null 时取根节点下全部子节点。</param>
        /// <returns>解析出的对象集合，顺序与原文中的节点顺序一致。</returns>
        /// <exception cref="XmlContentParseException">原文畸形、必填属性缺失、值无法转换或目标类型不受支持。</exception>
        public static IReadOnlyList<T> Parse<T>(string? content, string? elementName) where T : new()
        {
            // 空内容是正常路径：Apollo 未下发内容时 content 就是空串，
            // 此时返回空集合，由调用方决定是否沿用旧数据，而不是在这里抛。
            if (string.IsNullOrWhiteSpace(content))
            {
                return [];
            }

            XDocument document;
            try
            {
                // SetLineInfo 是报错里能给出行号的前提
                document = XDocument.Parse(content, LoadOptions.SetLineInfo);
            }
            catch (XmlException ex)
            {
                throw new XmlContentParseException(
                    $"解析 XML 内容失败：原文不是合法的 XML（第 {ex.LineNumber} 行第 {ex.LinePosition} 列）。",
                    lineNumber: ex.LineNumber,
                    linePosition: ex.LinePosition,
                    innerException: ex);
            }

            var root = document.Root;
            if (root is null)
            {
                return [];
            }

            var mappings = GetMappings(typeof(T));

            // 按 Elements() 取全部子节点而不是按名字取，
            // 导出的配置根节点与子节点常常同名，绑死名字会一条都取不到。
            var elements = string.IsNullOrEmpty(elementName)
                ? root.Elements()
                : root.Elements().Where(x => x.Name.LocalName == elementName);

            var result = new List<T>();
            var itemIndex = 0;
            foreach (var element in elements)
            {
                result.Add(MapElement<T>(element, mappings, itemIndex));
                itemIndex++;
            }

            return result;
        }

        /// <summary>
        /// 把单个 XML 节点映射为一个对象实例。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="element">当前节点。</param>
        /// <param name="mappings">该类型的映射关系。</param>
        /// <param name="itemIndex">当前节点在集合中的序号，用于报错定位。</param>
        /// <returns>填充完毕的对象实例。</returns>
        private static T MapElement<T>(XElement element, XmlPropertyMapping[] mappings, int itemIndex) where T : new()
        {
            var item = new T();
            var (line, position) = GetLineInfo(element);

            foreach (var mapping in mappings)
            {
                var attribute = element.Attribute(mapping.AttributeName);
                if (attribute is null)
                {
                    if (mapping.Required)
                    {
                        throw new XmlContentParseException(
                            $"解析 XML 内容失败：{BuildLocation(line, position, itemIndex)}缺少必填属性「{mapping.AttributeName}」。",
                            itemIndex, line, position, mapping.AttributeName, targetType: mapping.Property.PropertyType);
                    }

                    continue;
                }

                var raw = attribute.Value;

                // 可空类型上的空值按「没填」处理，直接留 null；
                // 非可空的值类型收不到空串，会走下面的转换并如实报错。
                if (mapping.IsNullable && raw.Length == 0)
                {
                    continue;
                }

                object value;
                try
                {
                    value = ConvertValue(raw, mapping.UnderlyingType);
                }
                catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
                {
                    throw new XmlContentParseException(
                        $"解析 XML 内容失败：{BuildLocation(line, position, itemIndex)}的属性「{mapping.AttributeName}」值 \"{raw}\" 无法转换为 {mapping.UnderlyingType}。",
                        itemIndex, line, position, mapping.AttributeName, raw, mapping.UnderlyingType, ex);
                }

                mapping.Property.SetValue(item, value);
            }

            return item;
        }

        /// <summary>
        /// 把 XML 上的文本值转换成目标类型。
        /// </summary>
        /// <param name="raw">属性的原始文本值。</param>
        /// <param name="targetType">目标类型（已剥掉 Nullable 包装）。</param>
        /// <returns>转换后的值。</returns>
        private static object ConvertValue(string raw, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return raw;
            }

            // 枚举同时接受名称与数值，且忽略大小写
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, raw, ignoreCase: true);
            }

            // 一律用不变区域，避免不同机器的区域设置把同一份配置解析成不同结果
            var culture = CultureInfo.InvariantCulture;
            return Type.GetTypeCode(targetType) switch
            {
                TypeCode.Boolean => ParseBoolean(raw),
                TypeCode.Char => char.Parse(raw),
                TypeCode.SByte => sbyte.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.Byte => byte.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.Int16 => short.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.UInt16 => ushort.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.Int32 => int.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.UInt32 => uint.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.Int64 => long.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.UInt64 => ulong.Parse(raw, NumberStyles.Integer, culture),
                TypeCode.Single => float.Parse(raw, NumberStyles.Float | NumberStyles.AllowThousands, culture),
                TypeCode.Double => double.Parse(raw, NumberStyles.Float | NumberStyles.AllowThousands, culture),
                TypeCode.Decimal => decimal.Parse(raw, NumberStyles.Number, culture),
                TypeCode.DateTime => DateTime.Parse(raw, culture, DateTimeStyles.None),
                _ => ConvertOtherValue(raw, targetType, culture),
            };
        }

        /// <summary>
        /// 转换没有对应 TypeCode 的受支持类型。
        /// </summary>
        /// <param name="raw">属性的原始文本值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="culture">用于解析的区域信息。</param>
        /// <returns>转换后的值。</returns>
        private static object ConvertOtherValue(string raw, Type targetType, CultureInfo culture)
        {
            if (targetType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(raw, culture, DateTimeStyles.None);
            }

            if (targetType == typeof(TimeSpan))
            {
                return TimeSpan.Parse(raw, culture);
            }

            if (targetType == typeof(Guid))
            {
                return Guid.Parse(raw);
            }

            // 建立映射时已经拦过不受支持的类型，走到这里说明两处判断不一致
            throw new XmlContentParseException($"解析 XML 内容失败：不受支持的目标类型 {targetType}。", targetType: targetType);
        }

        /// <summary>
        /// 【业务意图】布尔值除 true/false 外，还接受配置里常见的 1/0 写法。
        /// </summary>
        /// <param name="raw">属性的原始文本值。</param>
        /// <returns>转换后的布尔值。</returns>
        private static bool ParseBoolean(string raw)
        {
            if (bool.TryParse(raw, out var parsed))
            {
                return parsed;
            }

            return raw switch
            {
                "1" => true,
                "0" => false,
                _ => throw new FormatException($"\"{raw}\" 不是有效的布尔值。"),
            };
        }

        /// <summary>
        /// 取类型的映射关系，命中缓存则直接返回。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <returns>该类型上标注了映射特性的属性集合。</returns>
        private static XmlPropertyMapping[] GetMappings(Type type) => _mappingCache.GetOrAdd(type, BuildMappings);

        /// <summary>
        /// 反射扫描类型上标注了 <see cref="XmlAttributeNameAttribute"/> 的属性，并校验其可用性。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <returns>映射关系数组。</returns>
        private static XmlPropertyMapping[] BuildMappings(Type type)
        {
            var mappings = new List<XmlPropertyMapping>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = property.GetCustomAttribute<XmlAttributeNameAttribute>(inherit: true);
                if (attribute is null)
                {
                    continue;
                }

                if (property.SetMethod is null || !property.SetMethod.IsPublic)
                {
                    throw new XmlContentParseException(
                        $"建立 XML 映射失败：类型「{type.FullName}」的属性「{property.Name}」标注了映射特性，但没有公开的 set 访问器。",
                        attributeName: attribute.Name,
                        targetType: property.PropertyType);
                }

                var nullableUnderlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                var isNullable = nullableUnderlyingType is not null || !property.PropertyType.IsValueType;
                var underlyingType = nullableUnderlyingType ?? property.PropertyType;

                // 不受支持的类型在建立映射时就报错，而不是等跑到某一行数据才炸
                if (!IsSupported(underlyingType))
                {
                    throw new XmlContentParseException(
                        $"建立 XML 映射失败：类型「{type.FullName}」的属性「{property.Name}」的类型 {property.PropertyType} 不受支持，仅支持字符串、布尔、数值、DateTime、DateTimeOffset、TimeSpan、Guid、枚举及其可空形式。",
                        attributeName: attribute.Name,
                        targetType: property.PropertyType);
                }

                mappings.Add(new XmlPropertyMapping(property, attribute.Name, attribute.Required, underlyingType, isNullable));
            }

            return [.. mappings];
        }

        /// <summary>
        /// 判断目标类型是否在支持范围内。
        /// </summary>
        /// <param name="type">已剥掉 Nullable 包装的目标类型。</param>
        /// <returns>受支持返回 true。</returns>
        private static bool IsSupported(Type type) =>
            type == typeof(string)
            || type.IsEnum
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || Type.GetTypeCode(type) is not (TypeCode.Object or TypeCode.Empty or TypeCode.DBNull);

        /// <summary>
        /// 取节点在原文中的行列位置。
        /// </summary>
        /// <param name="element">当前节点。</param>
        /// <returns>行号与列号；取不到时均为 0。</returns>
        private static (int Line, int Position) GetLineInfo(XElement element) =>
            element is IXmlLineInfo info && info.HasLineInfo() ? (info.LineNumber, info.LinePosition) : (0, 0);

        /// <summary>
        /// 拼出错误信息里的定位描述，行信息缺失时退化为节点序号。
        /// </summary>
        /// <param name="line">行号。</param>
        /// <param name="position">列号。</param>
        /// <param name="itemIndex">节点序号（从 0 开始）。</param>
        /// <returns>可直接嵌进错误信息的定位描述。</returns>
        private static string BuildLocation(int line, int position, int itemIndex) =>
            line > 0
                ? $"第 {line} 行第 {position} 列（第 {itemIndex + 1} 个节点）"
                : $"第 {itemIndex + 1} 个节点";

        /// <summary>
        /// 单个属性的映射关系。
        /// </summary>
        /// <param name="property">目标类型上的属性。</param>
        /// <param name="attributeName">对应的 XML 属性名。</param>
        /// <param name="required">该 XML 属性是否必填。</param>
        /// <param name="underlyingType">已剥掉 Nullable 包装的目标类型。</param>
        /// <param name="isNullable">属性本身是否可为 null。</param>
        private sealed class XmlPropertyMapping(PropertyInfo property, string attributeName, bool required, Type underlyingType, bool isNullable)
        {
            /// <summary>【含义】目标类型上的属性。</summary>
            public PropertyInfo Property { get; } = property;

            /// <summary>【含义】对应的 XML 属性名。</summary>
            public string AttributeName { get; } = attributeName;

            /// <summary>【含义】该 XML 属性是否必填。</summary>
            public bool Required { get; } = required;

            /// <summary>【含义】已剥掉 Nullable 包装的目标类型。</summary>
            public Type UnderlyingType { get; } = underlyingType;

            /// <summary>【含义】属性本身是否可为 null，空值可直接跳过赋值。</summary>
            public bool IsNullable { get; } = isNullable;
        }
    }
}
