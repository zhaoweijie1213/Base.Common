using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.Processors.Contexts;
using QYQ.Base.Swagger;
using Xunit;

namespace QYQ.Base.Common.Test.Swagger
{
    /// <summary>
    /// 验证 Swagger XML 注释补充逻辑。
    /// </summary>
    public class SwaggerXmlCommentTests
    {
        /// <summary>
        /// 枚举成员没有 DescriptionAttribute 时应使用 XML Summary 注释。
        /// </summary>
        [Fact]
        public void Process_ShouldUseXmlSummary_WhenEnumFieldHasNoDescriptionAttribute()
        {
            var processor = new EnumProcessor(CreateXmlCommentProvider());
            var schema = CreateSchema(typeof(XmlOnlyEnum), processor);

            Assert.Contains("1:Enabled,启用状态;", schema.Description);
        }

        /// <summary>
        /// 枚举成员同时存在 DescriptionAttribute 和 XML Summary 时应优先使用特性说明。
        /// </summary>
        [Fact]
        public void Process_ShouldUseDescriptionAttributeFirst_WhenEnumFieldHasBothComments()
        {
            var processor = new EnumProcessor(CreateXmlCommentProvider());
            var schema = CreateSchema(typeof(AttributeFirstEnum), processor);

            Assert.Contains("1:Active,特性优先状态;", schema.Description);
            Assert.DoesNotContain("XML 状态说明", schema.Description);
        }

        /// <summary>
        /// 控制器缺少 Tag 说明时应使用控制器 XML Summary 注释。
        /// </summary>
        [Fact]
        public void Process_ShouldSetTagDescription_WhenControllerTagHasNoDescription()
        {
            var processor = new ControllerXmlCommentTagProcessor(CreateXmlCommentProvider());
            var document = new OpenApiDocument();
            document.Tags.Add(new OpenApiTag { Name = "Sample" });

            processor.Process(CreateDocumentProcessorContext(document, typeof(SampleController)));

            Assert.Equal("示例控制器说明", document.Tags.Single(item => item.Name == "Sample").Description);
        }

        /// <summary>
        /// 控制器已有显式 Tag 说明时不应被 XML Summary 覆盖。
        /// </summary>
        [Fact]
        public void Process_ShouldKeepExistingTagDescription_WhenTagHasDescription()
        {
            var processor = new ControllerXmlCommentTagProcessor(CreateXmlCommentProvider());
            var document = new OpenApiDocument();
            document.Tags.Add(new OpenApiTag { Name = "Sample", Description = "显式说明" });

            processor.Process(CreateDocumentProcessorContext(document, typeof(SampleController)));

            Assert.Equal("显式说明", document.Tags.Single(item => item.Name == "Sample").Description);
        }

        private static XmlCommentProvider CreateXmlCommentProvider()
        {
            return new XmlCommentProvider(new Dictionary<string, string>
            {
                [$"F:{GetXmlTypeName(typeof(XmlOnlyEnum))}.{nameof(XmlOnlyEnum.Enabled)}"] = "启用状态",
                [$"F:{GetXmlTypeName(typeof(AttributeFirstEnum))}.{nameof(AttributeFirstEnum.Active)}"] = "XML 状态说明",
                [$"T:{GetXmlTypeName(typeof(SampleController))}"] = "示例控制器说明"
            });
        }

        private static string GetXmlTypeName(Type type)
        {
            return (type.FullName ?? type.Name).Replace('+', '.');
        }

        private static JsonSchema CreateSchema(Type type, EnumProcessor processor)
        {
            var schema = new JsonSchema();
            var settings = new SystemTextJsonSchemaGeneratorSettings();
            var resolver = new JsonSchemaResolver(schema, settings);
            var generator = new JsonSchemaGenerator(settings);
            var context = new SchemaProcessorContext(type.ToContextualType(), schema, resolver, generator, settings);

            processor.Process(context);

            return schema;
        }

        private static DocumentProcessorContext CreateDocumentProcessorContext(OpenApiDocument document, Type controllerType)
        {
            var settings = new OpenApiDocumentGeneratorSettings();
            var schemaSettings = new SystemTextJsonSchemaGeneratorSettings();
            var resolver = new JsonSchemaResolver(document, schemaSettings);
            var generator = new JsonSchemaGenerator(schemaSettings);
            return new DocumentProcessorContext(
                document,
                new[] { controllerType },
                new[] { controllerType },
                resolver,
                generator,
                settings);
        }

        private enum XmlOnlyEnum
        {
            Enabled = 1
        }

        private enum AttributeFirstEnum
        {
            [Description("特性优先状态")]
            Active = 1
        }

        private class SampleController
        {
        }
    }
}
