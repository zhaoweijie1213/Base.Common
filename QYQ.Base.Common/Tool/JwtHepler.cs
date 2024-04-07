using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
        /// <returns></returns>
        public static string GenerateToken(UserInfo info,string secretKey,string issuer,string audience,int expires)
        {
            var claims = new[]
            {
                new Claim("UserId", info.UserId.ToString()),
                new Claim("UserName", info.Name),
                new Claim("HeadImg", info.HeadImg),
                new Claim("Phone", info.Phone),
                new Claim("Email", info.Email),
                new Claim("Sex", info.Sex),
                new Claim("Lobby", info.Lobby.ToString()),
                new Claim("TimeZone", info.TimeZone.ToString()),
                new Claim("Country", info.Country.ToString()),
                new Claim("Currency", info.Currency.ToString()),
                new Claim("Symbol", info.Symbol.ToString()),
                new Claim("ExtraProperties", JsonConvert.SerializeObject(info.ExtraProperties))
            };

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



    }
}
