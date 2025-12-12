using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
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
            var logger = options.LoggerFactory.CreateLogger<GrpcConsulResolverFactory>();

            string? name = null;
            if (options.ChannelOptions?.ServiceConfig?.Inner != null
                && options.ChannelOptions.ServiceConfig.Inner.TryGetValue("ServiceName", out var serviceName))
            {
                name = serviceName?.ToString();
            }
            else if (!string.IsNullOrWhiteSpace(options.Address?.Host))
            {
                name = options.Address.Host;
                logger.LogWarning("Create:未在ServiceConfig中找到ServiceName，使用地址主机名{ServiceName}", name);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                const string message = "Create:通道配置未包含ServiceName且地址主机名为空，无法创建Consul解析器";
                logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            var resolver = new GrpcConsulResolver(options.LoggerFactory)
            {
                ConsulClientOption = _consulOptions.CurrentValue,
                ServiceName = name
            };
            return resolver;
        }
    }
}
