using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QYQ.Base.Common.ApiResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;


        /// <summary>
        /// 
        /// </summary>
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlingMiddleware> _logger)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError("{path}:{message}\r\n{stackTrace}", context.Request.Path, ex.Message, ex.StackTrace);
                await HandleExceptionAsync(context, ex.Message);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, string msg)
        {
            ApiResult<string> data = new()
            {
                Code = context.Response.StatusCode,
                Data = null,
                Message = msg
            };
            //var result = JsonConvert.SerializeObject(data);
            //context.Response.ContentType = "application/json;charset=utf-8";
            //context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(data);
        }



    }

    /// <summary>
    /// 错误处理
    /// </summary>
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }

    }
}
