using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace QYQ.Base.Swagger
{
    /// <summary>
    /// 将接口归类到真实控制器 Tag，避免控制器级 OpenApiTag 误导 Swagger 分组。
    /// </summary>
    internal sealed class ControllerOperationTagProcessor : IOperationProcessor
    {
        /// <summary>
        /// 使用当前接口所属控制器名称重建 Swagger Operation Tags。
        /// </summary>
        /// <param name="context">Swagger 接口处理上下文。</param>
        /// <returns>返回 true 表示保留当前接口。</returns>
        public bool Process(OperationProcessorContext context)
        {
            var tagName = ControllerXmlCommentTagProcessor.GetControllerTagName(context.ControllerType);
            context.OperationDescription.Operation.Tags.Clear();
            context.OperationDescription.Operation.Tags.Add(tagName);
            return true;
        }
    }
}
