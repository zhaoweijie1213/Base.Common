using Newtonsoft.Json;
using QYQ.Base.Common.Models;
using QYQ.Base.Common.Models.Enum;
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
        /// 获取用户信息（兼容 Domestic / Overseas 两种 Token；健壮的 TryParse 解析）
        /// </summary>
        public static UserInfo GetUserInfoFromClaimsPrincipal(this ClaimsPrincipal claimsPrincipal)
        {
            var user = new UserInfo();

            // ===== 通用字段 =====
            if (long.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.UserId), out var uid))
                user.UserId = uid;

            user.Name = claimsPrincipal.FindFirstValue(CustomClaimTypes.UserName) ?? string.Empty;
            user.HeadImg = claimsPrincipal.FindFirstValue(CustomClaimTypes.HeadImg) ?? string.Empty;
            user.Phone = claimsPrincipal.FindFirstValue(CustomClaimTypes.Phone) ?? string.Empty;

            // ExtraProperties (JSON)
            var ext = claimsPrincipal.FindFirstValue(CustomClaimTypes.ExtraProperties);
            if (!string.IsNullOrWhiteSpace(ext))
                user.ExtraProperties = JsonConvert.DeserializeObject<ExtraProperties>(ext);

            // ===== 判定 Token 类型（0=Domestic,1=Overseas），缺失时按 Domestic 兜底 =====
            UserInfoTokenType tokenType = UserInfoTokenType.Domestic;
            var ttRaw = claimsPrincipal.FindFirstValue(CustomClaimTypes.TokenType);
            if (int.TryParse(ttRaw, out var ttVal) && System.Enum.IsDefined(typeof(UserInfoTokenType), ttVal))
                tokenType = (UserInfoTokenType)ttVal;

            // ===== 分类型提取差异字段 =====
            if (tokenType == UserInfoTokenType.Domestic)
            {
                user.Sex = claimsPrincipal.FindFirstValue(CustomClaimTypes.Sex) ?? string.Empty;
                user.Token = claimsPrincipal.FindFirstValue(CustomClaimTypes.Token) ?? string.Empty;

                // Config (JSON -> Dictionary<string,string>)
                var cfg = claimsPrincipal.FindFirstValue(CustomClaimTypes.Config);
                if (!string.IsNullOrWhiteSpace(cfg))
                    user.Config = JsonConvert.DeserializeObject<Dictionary<string, string>>(cfg) ?? [];

                // RealName: bool -> "True"/"False"
                if (bool.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.RealName), out var rn))
                    user.RealName = rn;

                if (int.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.PlayCount), out var pc))
                    user.PlayCount = pc;

                // PayMoney: 这里按 decimal 解析；如你的 UserInfo 定义是 double/long，请替换为对应类型
                if (decimal.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.PayMoney), out var pm))
                    user.PayMoney = pm;

            }
            else // Overseas
            {
                user.Email = claimsPrincipal.FindFirstValue(CustomClaimTypes.Email) ?? string.Empty;
                user.Country = claimsPrincipal.FindFirstValue(CustomClaimTypes.Country) ?? string.Empty;
                user.Currency = claimsPrincipal.FindFirstValue(CustomClaimTypes.Currency) ?? string.Empty;
                user.Symbol = claimsPrincipal.FindFirstValue(CustomClaimTypes.Symbol) ?? string.Empty;

                if (long.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.AppId), out var appId))
                    user.AppId = appId;

                if (double.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.TimeZone), out var tz))
                    user.TimeZone = tz;

                if (int.TryParse(claimsPrincipal.FindFirstValue(CustomClaimTypes.Lobby), out var lobby))
                    user.Lobby = lobby; // 你在海外 token 里也写入了 Lobby，这里一并解析
            }

            return user;
        }


        /// <summary>
        /// 获取 Token 类型（0=Domestic,1=Overseas），解析失败返回 null
        /// </summary>
        public static UserInfoTokenType? GetTokenType(this ClaimsPrincipal claimsPrincipal)
        {
            var raw = claimsPrincipal.FindFirstValue(CustomClaimTypes.TokenType);
            if (int.TryParse(raw, out var v) && System.Enum.IsDefined(typeof(UserInfoTokenType), v))
                return (UserInfoTokenType)v;
            return null;
        }

        private static string? FindFirstValue(this ClaimsPrincipal principal, string type)
            => principal?.Claims?.FirstOrDefault(c => c.Type == type)?.Value;

    }

 
}
