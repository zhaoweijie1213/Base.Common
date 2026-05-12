using QYQ.Base.Common.ApiResult;
using System.ComponentModel;

namespace Test.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class WeatherInput
    {
        /// <summary>
        /// 接口返回码
        /// </summary>
        public TestCode ResultCode { get; set; }
    }

    /// <summary>
    /// 测试枚举类型
    /// </summary>
    public enum TestCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("Success")]
        Success = 0,


        /// <summary>
        /// 系统错误
        /// </summary>
        [Description("Internal Server Error")]
        InternalServerError = 500,
    }

}
