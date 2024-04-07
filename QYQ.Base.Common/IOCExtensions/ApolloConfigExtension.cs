using Com.Ctrip.Framework.Apollo.Logging;
using Com.Ctrip.Framework.Apollo;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// apollo 扩展
    /// </summary>
    public static class ApolloConfigExtension
    {
        /// <summary>
        /// 添加apollo配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApolloConfigurationBuilder AddQYQApollo(this IConfigurationBuilder builder)
        {
            LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Information);
            var configuration = builder.Build();
            //获取apollo配置
            ApolloOptions apollo = configuration.GetSection("apollo").Get<ApolloOptions>();
            //读取集群
            if (!string.IsNullOrEmpty(configuration["Cluster"]))
            {
                apollo.Cluster = configuration["Cluster"];
            }

            //设置apollo的服务地址
            List<string> configServer = new()
            {
                apollo.Meta[apollo.Env.ToString()]
            };
            apollo.ConfigServer = configServer;
            return builder.AddApollo(apollo);
        }
    }
}
