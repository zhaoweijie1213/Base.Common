using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using QYQ.Base.Common.Enum;
using QYQ.Base.Common.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Tool
{
    /// <summary>
    /// jwt
    /// </summary>
    public class JwtHepler
    {
        /// <summary>
        /// 验证token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="jwtSettings"></param>
        /// <returns></returns>
        public static bool TokenVaildation(string accessToken, JwtSettings jwtSettings)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JsonWebTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = jwtSettings.Audience,
                ValidIssuer = jwtSettings.Issuer,
                IssuerSigningKey = creds.Key,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
            var result = tokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters).GetAwaiter().GetResult();

            //验证失败抛出错误
            if (!result.IsValid)
            {
                throw result.Exception;
            }
            //返回验证结果
            return result.IsValid;
        }


        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="info"></param>
        /// <param name="secretKey"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <param name="expires">过期时间(单位:秒)</param>
        /// <param name="tokenType">类型枚举: 根据类型的不同，token里包含的用户信息也不同</param>
        /// <returns></returns>
        public static string GenerateToken(UserInfo info,string secretKey,string issuer,string audience,int expires, UserInfoTokenType tokenType)
        {
            var claims = BuildClaims(info, tokenType);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(expires),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }


        private static IEnumerable<Claim> BuildClaims(UserInfo info, UserInfoTokenType tokenType)
        {
            // 公共字段（国内/海外都写入）
            var claims = new List<Claim>
            {
                new(CustomClaimTypes.TokenType, ((int)tokenType).ToString()),           // ⭐ 写入类型
                new(CustomClaimTypes.UserId,   info.UserId.ToString()),
                new(CustomClaimTypes.UserName, info.Name ?? string.Empty),
                new(CustomClaimTypes.HeadImg,  info.HeadImg ?? string.Empty),
                new(CustomClaimTypes.Phone, info.Phone),
                new(CustomClaimTypes.ExtraProperties, JsonConvert.SerializeObject(info.ExtraProperties ?? new()))
            };

            // 差异字段：按国内/海外分别写
            switch (tokenType)
            {
                case UserInfoTokenType.Domestic:
                    // 国内
                    claims.Add(new Claim(CustomClaimTypes.Sex, info.Sex ?? string.Empty));
                    claims.Add(new Claim(CustomClaimTypes.Config, JsonConvert.SerializeObject(info.Config)));
                    claims.Add(new Claim(CustomClaimTypes.RealName, info.RealName.ToString()));
                    claims.Add(new Claim(CustomClaimTypes.PlayCount, info.PlayCount.ToString()));
                    claims.Add(new Claim(CustomClaimTypes.PayMoney, info.PayMoney.ToString()));
                    break;

                case UserInfoTokenType.Overseas:
                    // 海外：常见需要邮箱、国家、货币、货币符号、时区等
                    claims.Add(new Claim(CustomClaimTypes.Email, info.Email ?? string.Empty));
                    claims.Add(new Claim(CustomClaimTypes.Country, info.Country ?? string.Empty));
                    claims.Add(new Claim(CustomClaimTypes.Currency, info.Currency ?? string.Empty));
                    claims.Add(new Claim(CustomClaimTypes.Symbol, info.Symbol ?? string.Empty));
                    claims.Add(new Claim(CustomClaimTypes.TimeZone, info.TimeZone.ToString()));
                    claims.Add(new Claim(CustomClaimTypes.AppId, info.AppId.ToString()));
                    claims.Add(new Claim(CustomClaimTypes.Lobby, info.Lobby.ToString()));
                    // 海外可选：手机号/性别/大厅不一定需要，如要兼容可一并写入
                    break;
            }

            return claims;
        }

        /// <summary>
        /// 解析token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static ClaimsPrincipal ParseToken(string token, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }


        /// <summary>
        /// 读取 JWT的信息(不进行任何验证)
        /// </summary>
        /// <param name="jwtToken"></param>
        public static JwtSecurityToken? ReadJwt(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtTokenRead = tokenHandler.ReadJwtToken(jwtToken);
            return jwtTokenRead;
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="jsonWebKey"></param>
        /// <returns></returns>
        public static async Task<bool> ValidateJwtToken(string jwtToken, JsonWebKey jsonWebKey)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jsonWebKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                //ClockSkew = TimeSpan.Zero, // 默认情况下有5分钟的时差容忍
                ValidateLifetime = true
            };

            //var tokenHandler = new JwtSecurityTokenHandler();

            var tokenHandler = new JsonWebTokenHandler();

            var result = await tokenHandler.ValidateTokenAsync(jwtToken, tokenValidationParameters);

            //验证失败抛出错误
            if (!result.IsValid)
            {
                throw result.Exception;
            }

            return result.IsValid;
        }

    }
}
