using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace QYQ.Base.Swagger
{
    /// <summary>
    /// 将控制器 XML Summary 注释补充到 Swagger Tag 说明。
    /// </summary>
    internal sealed class ControllerXmlCommentTagProcessor : IDocumentProcessor
    {
        private readonly IXmlCommentProvider _xmlCommentProvider;

        /// <summary>
        /// 创建控制器 Tag 注释处理器。
        /// </summary>
        /// <param name="xmlCommentProvider">XML 注释读取器。</param>
        public ControllerXmlCommentTagProcessor(IXmlCommentProvider xmlCommentProvider)
        {
            _xmlCommentProvider = xmlCommentProvider;
        }

        /// <summary>
        /// 将缺失说明的控制器 Tag 填充为控制器 XML Summary。
        /// </summary>
        /// <param name="context">Swagger 文档处理上下文。</param>
        public void Process(DocumentProcessorContext context)
        {
            var operationTagNames = context.Document.Operations
                .SelectMany(item => item.Operation.Tags)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToHashSet(StringComparer.Ordinal);

            RemoveUnusedDocumentTags(context, operationTagNames);

            foreach (var controllerType in context.ControllerTypes)
            {
                var description = _xmlCommentProvider.GetSummary(controllerType);
                if (string.IsNullOrWhiteSpace(description))
                {
                    continue;
                }

                var tagName = GetControllerTagName(controllerType);
                if (!operationTagNames.Contains(tagName))
                {
                    continue;
                }

                var tag = context.Document.Tags.FirstOrDefault(item => item.Name == tagName);
                if (tag == null)
                {
                    context.Document.Tags.Add(new OpenApiTag
                    {
                        Name = tagName,
                        Description = description
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(tag.Description))
                {
                    tag.Description = description;
                }
            }
        }

        private static void RemoveUnusedDocumentTags(DocumentProcessorContext context, HashSet<string> operationTagNames)
        {
            var unusedTags = context.Document.Tags
                .Where(item => !operationTagNames.Contains(item.Name))
                .ToList();

            foreach (var unusedTag in unusedTags)
            {
                context.Document.Tags.Remove(unusedTag);
            }
        }

        internal static string GetControllerTagName(Type controllerType)
        {
            const string controllerSuffix = "Controller";
            return controllerType.Name.EndsWith(controllerSuffix, StringComparison.Ordinal)
                ? controllerType.Name[..^controllerSuffix.Length]
                : controllerType.Name;
        }
    }
}
