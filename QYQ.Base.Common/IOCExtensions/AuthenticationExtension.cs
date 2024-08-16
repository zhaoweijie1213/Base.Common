using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QYQ.Base.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// 鉴权
    /// </summary>
    public static class AuthenticationExtension
    {
        /// <summary>
        /// 添加鉴权
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddQYQAuthentication(builder.Configuration);
            return builder;
        }


        /// <summary>
        /// 添加身份验证
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddQYQAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtseting = configuration.GetSection("jwt").Get<JwtSettings>() ?? throw new NullReferenceException("JWT config is null!");
            services.Configure<JwtSettings>(configuration.GetSection("jwt"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {

                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true, //是否验证Issuer
                       ValidateAudience = true, //是否验证Audience
                       ValidateLifetime = true,  //是否验证失效时间  当设置exp和nbf时有效
                       ValidateIssuerSigningKey = true,  ////是否验证密钥 SecurityKey
                       ValidAudience = jwtseting.Audience,//Audience
                       ValidIssuer = jwtseting.Issuer,//Issuer，这两项和登陆时颁发的一致
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtseting.SecretKey)),     //拿到SecurityKey
                       //// 验证过期时间的委托，自定义验证规则
                       //LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) => expires > DateTime.UtcNow,
                       ClockSkew = TimeSpan.FromSeconds(0)   //设置toekn过期之后立马失效 默认是5分钟
                   };
                   options.SaveToken = true;
               });

            return services;
        }

    }
}
