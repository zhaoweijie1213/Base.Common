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
                UserId = Convert.ToInt64(claimsPrincipal.FindFirstValue(CustomClaimTypes.UserId)),
                Name = claimsPrincipal.FindFirstValue(CustomClaimTypes.UserName) ?? "",
                Email = claimsPrincipal.FindFirstValue(CustomClaimTypes.Email) ?? "",
                HeadImg = claimsPrincipal.FindFirstValue(CustomClaimTypes.HeadImg) ?? "",
                Phone = claimsPrincipal.FindFirstValue(CustomClaimTypes.Phone) ?? "",
                Sex = claimsPrincipal.FindFirstValue(CustomClaimTypes.Sex) ?? "",
                Lobby = Convert.ToInt32(claimsPrincipal.FindFirstValue(CustomClaimTypes.Lobby)),
                TimeZone = Convert.ToDouble(claimsPrincipal.FindFirstValue(CustomClaimTypes.TimeZone)),
                Country = claimsPrincipal.FindFirstValue(CustomClaimTypes.Country) ?? "",
                Currency = claimsPrincipal.FindFirstValue(CustomClaimTypes.Currency) ?? "",
                Symbol = claimsPrincipal.FindFirstValue(CustomClaimTypes.Symbol) ?? "",
                AppId = Convert.ToInt64(claimsPrincipal.FindFirstValue(CustomClaimTypes.AppId))
            };
            var extra = claimsPrincipal.FindFirstValue(CustomClaimTypes.ExtraProperties);
            if(!string.IsNullOrEmpty(extra))
            {
                user.ExtraProperties = JsonConvert.DeserializeObject<ExtraProperties>(extra);
            }
            var config = claimsPrincipal.FindFirstValue(CustomClaimTypes.Config);
            if (!string.IsNullOrEmpty(config))
            {
                user.Config = JsonConvert.DeserializeObject<Dictionary<string, string>>(config) ?? [];
            }
            return user;
        }
    }

 
}
