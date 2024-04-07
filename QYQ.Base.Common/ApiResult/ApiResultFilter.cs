using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// 返回结果处理
    /// </summary>
    public class ApiResultFilter : ActionFilterAttribute
    {
        /// <summary>
        /// 绑定模型参数错误处理
        /// </summary>
        /// <param name="context"></param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //模型验证
            if (!context.ModelState.IsValid)
            {

                List<string> message = new() {  };
                foreach (var item in context.ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        message.Add(error.ErrorMessage);
                    }
                }
                ApiResult<object> result = new()
                {
                    Data = null,
                    Code = (int)ApiResultCode.ErrorParams,
                    Message = string.Join(",", message)

                };
                //var reader = new StreamReader(context.HttpContext.Request.Body);
                //var contentFromBody = reader.ReadToEnd();
                ObjectResult objectResult = new(result);
                context.Result = objectResult;
            }
            base.OnResultExecuting(context);
        }

        ///// <summary>
        ///// Action执行完成,返回结果错误处理
        ///// </summary>
        ///// <param name="actionExecutedContext"></param>
        //public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        //{
        //    if (actionExecutedContext.Exception != null)
        //    {
        //        ApiResult<object> robj = new ApiResult<object>()
        //        {
        //            message = actionExecutedContext.Exception.Message,
        //            code = (int)ApiResultCode.UnknownError
        //        };
        //        ObjectResult objectResult = new ObjectResult(robj);
        //        actionExecutedContext.Result = objectResult;
        //    }
        //    base.OnActionExecuted(actionExecutedContext);
        //}
    }
}
