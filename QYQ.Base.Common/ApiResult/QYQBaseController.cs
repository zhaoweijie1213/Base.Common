using Microsoft.AspNetCore.Mvc;
using QYQ.Base.Common.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// 基础类
    /// </summary>
    public class QYQBaseController : ControllerBase
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code">状态码</param>
        /// <param name="data">返回数据</param>
        /// <param name="msg">说明信息,如果为空，则返回枚举描述信息</param>
        /// <returns></returns>
        protected static ApiResult<T> Json<T>(ApiResultCode code, T data, string msg = "")
        {
            //如果message为空,默认返回枚举的描述
            if (string.IsNullOrEmpty(msg))
            {
                msg = code.GetDescription();
            }
            var result = new ApiResult<T>()
            {
                Code = (int)code,
                Message = msg,
                Data = data
            };
            return result;
        }


        /// <summary>
        /// 返回结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code">状态码</param>
        /// <param name="data">返回数据</param>
        /// <param name="msg">说明信息</param>
        /// <returns></returns>
        protected static ApiResult<T> Json<T>(int code, T data, string msg)
        {
            var result = new ApiResult<T>()
            {
                Code = (int)code,
                Message = msg,
                Data = data
            };
            return result;
        }
    }

}
