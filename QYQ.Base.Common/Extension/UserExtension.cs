using Newtonsoft.Json;
using QYQ.Base.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Extension
{
    /// <summary>
    /// 用户扩展
    /// </summary>
    public static class UserExtension
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public static UserInfo GetUserInfoFromClaimsPrincipal(this ClaimsPrincipal claimsPrincipal)
        {
            var user = new UserInfo
            {
                UserId = Convert.ToInt64(claimsPrincipal.FindFirstValue("UserId")),
                Name = claimsPrincipal.FindFirstValue("UserName") ?? "",
                Email = claimsPrincipal.FindFirstValue("Email") ?? "",
                HeadImg = claimsPrincipal.FindFirstValue("HeadImg") ?? "",
                Phone = claimsPrincipal.FindFirstValue("Phone") ?? "",
                Sex = claimsPrincipal.FindFirstValue("Sex") ?? "",
                Lobby = Convert.ToInt32(claimsPrincipal.FindFirstValue("Lobby")),
                TimeZone = Convert.ToDouble(claimsPrincipal.FindFirstValue("TimeZone")),
                Country = claimsPrincipal.FindFirstValue("Country") ?? "",
                Currency = claimsPrincipal.FindFirstValue("Currency") ?? "",
                Symbol = claimsPrincipal.FindFirstValue("Symbol") ?? "",
                AppId = Convert.ToInt64(claimsPrincipal.FindFirstValue("AppId"))
            };
            var extra = claimsPrincipal.FindFirstValue("ExtraProperties");
            if(!string.IsNullOrEmpty(extra))
            {
                user.ExtraProperties = JsonConvert.DeserializeObject<ExtraProperties>(extra);
            }
            return user;
        }
    }

 
}
