using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QYQ.Base.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace QYQ.Base.Common.Middleware
{
    /// <summary>
    /// Http日志中间件
    /// </summary>
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 配置
        /// </summary>
        protected IOptions<QYQHttpLoggingOptions> _httpLoggingOptions;

        /// <summary>
        /// 
        /// </summary>
        public HttpLoggingMiddleware(RequestDelegate next, IOptions<QYQHttpLoggingOptions> options)
        {
            _next = next;
            _httpLoggingOptions = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, ILogger<HttpLoggingMiddleware> logger)
        {
            //过滤配置的路径
            if (!_httpLoggingOptions.Value.IgnorePath.Any(i => context.Request.Path.ToString().Contains(i)) && context.Request.Path.ToString().Contains("/api") && logger.IsEnabled(LogLevel.Information))
            {
                var watch = new Stopwatch();
                watch.Start();

                var requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:fff");

                var requestBodyContent = "";
                var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault();
                if (string.IsNullOrEmpty(requestId))
                {
                    requestId = context.TraceIdentifier;
                }

                // 捕获请求
                // 检查请求体是否可读;请求体大小限制
                if (context.Request.ContentType != null && context.Request.ContentLength > 0 && context.Request.ContentLength < _httpLoggingOptions.Value.MaxRequestBodySize)
                {
                    context.Request.EnableBuffering(); // 使请求体可读
                    requestBodyContent = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    context.Request.Body.Position = 0; // 重置流位置

                    //// 检查内容类型
                    //if (context.Request.HasJsonContentType())
                    //{
                    //    var bodyObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                    //    requestBodyContent = JsonConvert.SerializeObject(bodyObj); // JSON数据
                    //}
                    //else if (context.Request.HasFormContentType && !context.Request.ContentType.Contains("multipart/form-data"))
                    //{
                    //    //var form = context.Request.Form; // 获取已解析的表单数据
                    //    //requestBodyContent = JsonConvert.SerializeObject(form); // JSON数据
                    //    requestBodyContent = body;
                    //}
                }

                // 准备捕获响应
                var originalBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                // 继续处理其他中间件
                await _next(context);

                // 检查响应长度
                var responseLength = context.Response.ContentLength ?? responseBodyStream.Length;
                var isLargeResponse = responseLength > _httpLoggingOptions.Value.MaxResponseLenght; // 限制大小

                // 捕获响应
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBodyText = "";
                if (responseLength > 0 && !isLargeResponse)
                {
                    responseBodyText = await new StreamReader(responseBodyStream).ReadToEndAsync();
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                }
        
                await responseBodyStream.CopyToAsync(originalBodyStream);  // 将响应写回原始流

                watch.Stop();

                // 构建日志信息
                var logBuilder = new StringBuilder();
                logBuilder.AppendLine($"Request Time:{requestTime} {context.Request.Method} {context.Response.StatusCode} - {watch.ElapsedMilliseconds}ms {context.Request.Protocol}" +
                    $" Query:{context.Request.Path}{HttpUtility.UrlDecode(context.Request.QueryString.Value)} Content-Type:{context.Request.ContentType}" +
                    $" Request-ID:{requestId}" +
                    $" Request Body: {requestBodyContent}" +
                    $" Response Body: {responseBodyText}");

                // 打印日志
                if (watch.ElapsedMilliseconds > _httpLoggingOptions.Value.LongQueryThreshold)
                {
                    logger.LogWarning(logBuilder.ToString());
                }
                else
                {
                    logger.LogInformation(logBuilder.ToString());
                }
                
            }
            else
            {
                // 继续处理其他中间件
                await _next(context);
            }
        }

    }
}
