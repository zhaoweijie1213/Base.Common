using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Models
{

    /// <summary>
    /// 用户信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public long UserId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// 邮箱
        /// </summary>
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 手机
        /// </summary>
        [JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        [JsonProperty("sex", NullValueHandling = NullValueHandling.Ignore)]
        public string Sex { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        [JsonProperty("headImg", NullValueHandling = NullValueHandling.Ignore)]
        public string HeadImg { get; set; } = string.Empty;

        /// <summary>
        /// 大厅
        /// </summary>
        [JsonProperty("lobby", NullValueHandling = NullValueHandling.Ignore)]
        public int Lobby { get; set; }

        /// <summary>
        /// 时区
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public double TimeZone { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// 币种
        /// </summary>
        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// 货币符号
        /// </summary>
        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// 扩展属性
        /// </summary>
        [JsonProperty("extraProperties", NullValueHandling = NullValueHandling.Ignore)]
        public ExtraProperties? ExtraProperties { get; set; }

    }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public class ExtraProperties
    {
        /// <summary>
        /// 账号
        /// </summary>
        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public string Account { get; set;} = string.Empty;

        /// <summary>
        /// facebookId
        /// </summary>
        [JsonProperty("facebookId", NullValueHandling = NullValueHandling.Ignore)]
        public string FacebookId { get; set; } = string.Empty;
    }

}
