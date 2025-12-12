using Consul;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace QYQ.Base.Consul.Grpc.Resolve
{
    /// <summary>
    /// Grpc自定义解析程序
    /// </summary>
    /// <remarks>
    /// _consulClientOption
    /// </remarks>
    public class GrpcConsulResolver(ILoggerFactory loggerFactory) : PollingResolver(loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<GrpcConsulResolver>();

        //private readonly Uri _address;
        //private readonly int _port;

        /// <summary>
        /// 
        /// </summary>
        public ConsulServiceOptions? ConsulClientOption { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        private System.Threading.Timer? _timer;

        private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStarted()
        {
            base.OnStarted();

            if (_refreshInterval != Timeout.InfiniteTimeSpan)
            {
                _timer = new System.Threading.Timer(OnTimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _timer.Change(_refreshInterval, _refreshInterval);
            }
        }

        /// <summary>
        /// 获取Consul地址列表
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ResolveAsync(CancellationToken cancellationToken)
        {
            if (ConsulClientOption == null)
            {
                const string message = "ResolveAsync:ConsulClientOption未配置，无法解析服务地址";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }
            using ConsulClient client = new(c =>
            {
                c.Address = new Uri(ConsulClientOption.ConsulAddress);
                c.Token = ConsulClientOption.Token;
            });
            //consul实例获取
            var entrys = await client.Health.Service(ServiceName, tag: "", passingOnly: true, cancellationToken);
            var addresses = entrys.Response
                .Select(entry =>
                {
                    var address = entry.Service.Address;
                    return new BalancerAddress(address, entry.Service.Port);
                })
                .ToArray();
            if (addresses.Length == 0)
            {
                _logger.LogWarning("ResolveAsync:解析器没有返回{ServiceName}服务地址", ServiceName);
            }
            // 解析结果通知负载均衡器
            Listener(ResolverResult.ForResult(addresses));
        }

        private void OnTimerCallback(object? state)
        {
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                _logger.LogError("OnTimerCallback:{message}\r\n{stackTrace}", ex.Message, ex.StackTrace);
            }
        }
    }
}
