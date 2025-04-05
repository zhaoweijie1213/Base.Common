using EasyCaching.Redis;
using EasyCaching.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QYQ.Base.SnowId.Interface;
using QYQ.Base.SnowId.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// 容器扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 默认雪花id生成
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultSnowIdGenerator(this IServiceCollection services, Action<SnowIdOptions>? action = null)
        {
            if (action != null)
            {
                services.Configure(action);
            }
            services.AddSingleton<ISnowIdGenerator, SnowIdDefaultGenerator>();
            return services;
        }

        /// <summary>
        ///  自动注册workerId   添加雪花id生成 (需要redis配置)
        ///  "Redis":{"Password":"YRBoWMgaziuALOU","AllowAdmin":true,"Endpoints":[{"Host":"192.168.0.224","Port":6379}],"Database":0}
        /// </summary>
        /// <param name="services"></param>
        /// <param name="redis"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IServiceCollection AddSnowIdRedisGenerator(this IServiceCollection services, RedisDBOptions? redis)
        {
            services.AddSingleton<ISnowIdGenerator, SnowIdRedisGenerator>();
            services.AddSingleton<WorkerIdManager>();

            if (redis == null)
            {
                throw new Exception("请添加Redis配置!");
            }

            #region EasyCaching注册

            services.AddEasyCaching(options =>
            {
                Action<EasyCachingJsonSerializerOptions> easycaching = x =>
                {
                    x.NullValueHandling = NullValueHandling.Ignore;
                    x.TypeNameHandling = TypeNameHandling.None;
                };
                options.UseRedis(config =>
                {
                    config.DBConfig = redis;
                }, "SnowIdRedis").WithJson(easycaching, "SnowIdRedis");
            });

            #endregion


            services.AddHostedService<WorkerIdWorkerService>();

            return services;
        }
    }
}
