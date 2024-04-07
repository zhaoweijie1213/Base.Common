using QYQ.Base.Common.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// api返回结果对象
    /// ApiResult
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 提示消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 结果
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 设置返回结果
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        public ApiResult<T> SetRsult(int code, T? data, string message = "success")
        {
            Code = code;
            Data = data;
            Message = message;
            return this;
        }

        /// <summary>
        /// 设置返回结果
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        public ApiResult<T> SetRsult(ApiResultCode code, T? data, string message)
        {
            Code = (int)code;
            Data = data;
            Message = message;
            return this;
        }

        /// <summary>
        /// 设置返回结果
        /// 返回枚举描述信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        public ApiResult<T> SetRsult(ApiResultCode code, T? data)
        {
            Code = (int)code;
            Data = data;
            Message = code.GetDescription();
            return this;
        }
    }
}
