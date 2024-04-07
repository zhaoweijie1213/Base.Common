using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.Grpc.Resolve
{
    /// <summary>
    /// 负载均衡工厂
    /// </summary>
    public class GrpcConsulResolverFactory : ResolverFactory
    {
        /// <summary>
        /// 解析scheme为'consul'的Url地址
        /// </summary>
        public override string Name => "consul";

        private readonly IOptionsMonitor<ConsulServiceOptions> _consulOptions;

        /// <summary>
        /// 
        /// </summary>
        public GrpcConsulResolverFactory(IOptionsMonitor<ConsulServiceOptions> consulOptions)
        {
            _consulOptions = consulOptions;
        }

        /// <summary>
        /// 创建负载均衡器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override Resolver Create(ResolverOptions options)
        {
            string name = options.ChannelOptions.ServiceConfig.Inner["ServiceName"].ToString();
            var resolver = new GrpcConsulResolver(options.LoggerFactory)
            {
                ConsulClientOption = _consulOptions.CurrentValue,
                ServiceName = name
            };
            return resolver;
        }
    }
}
