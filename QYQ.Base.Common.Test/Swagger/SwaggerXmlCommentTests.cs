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

            Assert.Contains("Enabled = 1（启用状态）;", schema.Description);
        }

        /// <summary>
        /// 枚举成员同时存在 DescriptionAttribute 和 XML Summary 时应只显示 XML Summary 注释。
        /// </summary>
        [Fact]
        public void Process_ShouldUseXmlSummaryOnly_WhenEnumFieldHasDescriptionAttribute()
        {
            var processor = new EnumProcessor(CreateXmlCommentProvider());
            var schema = CreateSchema(typeof(AttributeFirstEnum), processor);

            Assert.Contains("Active = 1（XML 状态说明）;", schema.Description);
            Assert.DoesNotContain("特性优先状态", schema.Description);
        }

        /// <summary>
        /// 枚举成员没有 XML Summary 注释时应只显示枚举名和值。
        /// </summary>
        [Fact]
        public void Process_ShouldUseEnumNameAndValue_WhenEnumFieldHasNoXmlSummary()
        {
            var processor = new EnumProcessor(CreateXmlCommentProvider());
            var schema = CreateSchema(typeof(NoXmlSummaryEnum), processor);

            Assert.Contains("Pending = 2;", schema.Description);
        }

        /// <summary>
        /// 引用枚举的模型属性不应重复追加枚举说明。
        /// </summary>
        [Fact]
        public void Process_ShouldNotAppendEnumDescriptionToReferenceProperty()
        {
            var processor = new EnumProcessor(CreateXmlCommentProvider());
            var enumSchema = CreateSchema(typeof(XmlOnlyEnum), processor);
            var classSchema = new JsonSchema();
            classSchema.Properties[nameof(ModelWithEnum.Code)] = new JsonSchemaProperty
            {
                Description = "接口返回码",
                Reference = enumSchema
            };

            ProcessSchema(typeof(ModelWithEnum), classSchema, processor);

            Assert.Equal("接口返回码", classSchema.Properties[nameof(ModelWithEnum.Code)].Description);
        }

        /// <summary>
        /// 控制器缺少 Tag 说明时应使用控制器 XML Summary 注释。
        /// </summary>
        [Fact]
        public void Process_ShouldSetTagDescription_WhenControllerTagHasNoDescription()
        {
            var processor = new ControllerXmlCommentTagProcessor(CreateXmlCommentProvider());
            var document = CreateDocumentWithOperationTag("Sample");
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
            var document = CreateDocumentWithOperationTag("Sample");
            document.Tags.Add(new OpenApiTag { Name = "Sample", Description = "显式说明" });

            processor.Process(CreateDocumentProcessorContext(document, typeof(SampleController)));

            Assert.Equal("显式说明", document.Tags.Single(item => item.Name == "Sample").Description);
        }

        /// <summary>
        /// 没有实际接口引用的 Swagger Tag 不应显示为空分组。
        /// </summary>
        [Fact]
        public void Process_ShouldRemoveTag_WhenNoOperationUsesIt()
        {
            var processor = new ControllerXmlCommentTagProcessor(CreateXmlCommentProvider());
            var document = CreateDocumentWithOperationTag("Sample");
            document.Tags.Add(new OpenApiTag { Name = "Sample" });
            document.Tags.Add(new OpenApiTag { Name = "V3VersionedValues", Description = "空分组说明" });

            processor.Process(CreateDocumentProcessorContext(document, typeof(SampleController)));

            Assert.DoesNotContain(document.Tags, item => item.Name == "V3VersionedValues");
            Assert.Contains(document.Tags, item => item.Name == "Sample");
        }

        /// <summary>
        /// 控制器没有实际接口引用时不应主动新增空 Tag。
        /// </summary>
        [Fact]
        public void Process_ShouldNotCreateTag_WhenNoOperationUsesControllerTag()
        {
            var processor = new ControllerXmlCommentTagProcessor(CreateXmlCommentProvider());
            var document = new OpenApiDocument();

            processor.Process(CreateDocumentProcessorContext(document, typeof(SampleController)));

            Assert.Empty(document.Tags);
        }

        /// <summary>
        /// 接口存在错误 Tag 时应按真实控制器名称重建分组。
        /// </summary>
        [Fact]
        public void Process_ShouldReplaceOperationTag_WithControllerName()
        {
            var processor = new ControllerOperationTagProcessor();
            var context = CreateOperationProcessorContext(typeof(SampleController), "V3VersionedValues");

            var result = processor.Process(context);

            Assert.True(result);
            Assert.Equal(new[] { "Sample" }, context.OperationDescription.Operation.Tags);
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
            ProcessSchema(type, schema, processor);
            return schema;
        }

        private static void ProcessSchema(Type type, JsonSchema schema, EnumProcessor processor)
        {
            var settings = new SystemTextJsonSchemaGeneratorSettings();
            var resolver = new JsonSchemaResolver(schema, settings);
            var generator = new JsonSchemaGenerator(settings);
            var context = new SchemaProcessorContext(type.ToContextualType(), schema, resolver, generator, settings);

            processor.Process(context);
        }

        private static OpenApiDocument CreateDocumentWithOperationTag(string tagName)
        {
            var document = new OpenApiDocument();
            var operation = new OpenApiOperation();
            operation.Tags.Add(tagName);
            var pathItem = new OpenApiPathItem();
            pathItem[OpenApiOperationMethod.Get] = operation;
            document.Paths["/sample"] = pathItem;
            return document;
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

        private static OperationProcessorContext CreateOperationProcessorContext(Type controllerType, string tagName)
        {
            var document = new OpenApiDocument();
            var operationDescription = new OpenApiOperationDescription
            {
                Path = "/sample",
                Method = OpenApiOperationMethod.Get,
                Operation = new OpenApiOperation()
            };
            operationDescription.Operation.Tags.Add(tagName);
            var settings = new OpenApiDocumentGeneratorSettings();
            var schemaSettings = new SystemTextJsonSchemaGeneratorSettings();
            var resolver = new JsonSchemaResolver(document, schemaSettings);

            return new OperationProcessorContext(
                document,
                operationDescription,
                controllerType,
                controllerType.GetMethod(nameof(SampleController.Get))!,
                null!,
                resolver,
                settings,
                new[] { operationDescription });
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

        private enum NoXmlSummaryEnum
        {
            [Description("等待处理")]
            Pending = 2
        }

        private class ModelWithEnum
        {
            public XmlOnlyEnum Code { get; set; }
        }

        private class SampleController
        {
            public void Get()
            {
            }
        }
    }
}
