using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QYQ.Base.Consul.Grpc.Serivce
{
    /// <summary>
    /// consul 注册 和 注销
    /// </summary>
    public class ConsulGRPCHostedService(ILogger<ConsulGRPCHostedService> logger,IConsulClient consulClient, IConfiguration configuration) : IHostedService
    {
        /// <summary>
        /// consul客户端
        /// </summary>
        private readonly IConsulClient _consulClient = consulClient;

        /// <summary>
        /// consul服务ID
        /// </summary>
        private string ServiceId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var _serviceOptions = configuration.GetSection("ConsulOptions").Get<ConsulServiceOptions>();
            var agent = _serviceOptions.ConsulAgents.FirstOrDefault(i => i.AgentCategory == AgentCategory.GRPC) ?? throw new ArgumentException("Consul agent configuration not found");
            var ipAddress = NetworkUtil.GetHostIPv4(_serviceOptions.HostIPAddress);
            logger.LogInformation("StartAsync:Consul注册当前IP地址{ipAddress}", ipAddress);
            int port = agent.Port;
            agent.Meta.Add("Env", configuration["apollo:Env"]);

            // 生成一个唯一的服务ID，通常可以用服务名+IP+端口等方式保证全局唯一
            ServiceId = $"{agent.ServiceName}-{Environment.MachineName}-{ipAddress}-{port}";
            // 在启动时移除相同地址和端口的旧服务
            //await ServicesDeregisterAsync();

            var registration = new AgentServiceRegistration
            {
                ID = ServiceId,
                Name = agent.ServiceName,
                Address = ipAddress,
                Port = port,
                Tags = agent.Tags,
                Meta = agent.Meta,
                Check = new AgentServiceCheck
                {
                    //gRPC特有
                    GRPC = $"{ipAddress}:{agent.Port}",
                    //支持http
                    GRPCUseTLS = false,
                    // 注册超时
                    Timeout = TimeSpan.FromSeconds(5),
                    // 服务停止多久后注销服务
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    // 健康检查时间间隔
                    Interval = TimeSpan.FromSeconds(10),
                }
            };

            //注册服务
            await _consulClient.Agent.ServiceRegister(registration, replaceExistingChecks: true, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 在停止时注销服务
                await _consulClient.Agent.ServiceDeregister(ServiceId, CancellationToken.None);
                //移除重复注册的旧服务
                await ServicesDeregisterAsync();
            }
            catch (Exception e)
            {
                logger.LogError("StopAsync:{Message}\r\n{StackTrace}", e.Message, e.StackTrace);
            }
     
        }


        /// <summary>
        /// 移除重复注册的旧服务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ServicesDeregisterAsync()
        {
            var _serviceOptions = configuration.GetSection("ConsulOptions").Get<ConsulServiceOptions>();
            var agent = _serviceOptions.ConsulAgents.FirstOrDefault(i => i.AgentCategory == AgentCategory.GRPC) ?? throw new ArgumentException("Consul agent configuration not found");
            var ipAddress = NetworkUtil.GetHostIPv4(_serviceOptions.HostIPAddress);
            logger.LogInformation("ServicesDeregisterAsync:Consul注册当前IP地址{ipAddress}", ipAddress);
            int port = agent.Port;
            // 移除相同地址和端口的旧服务
            var healthResult = await _consulClient.Health.Service(agent.ServiceName, tag: "", passingOnly: true);
            var healthyInstances = healthResult.Response;
            logger.LogInformation("Consul服务列表: {services}", JsonConvert.SerializeObject(healthyInstances));
            foreach (var entry in healthyInstances)
            {
                //var service = entry.Service;
                if (entry.Service.Address == ipAddress && entry.Service.Port == port && entry.Service.Service.Equals(agent.ServiceName, StringComparison.OrdinalIgnoreCase) && entry.Service.ID != ServiceId)
                {
                    logger.LogInformation($"Service Name:{entry.Service.Service},Service ID: {entry.Service.ID}, Address: {entry.Service.Address}, Port: {entry.Service.Port}");
                    try
                    {
                        var node = entry.Node;
                        // ③ Catalog 级删除
                        var dereg = new CatalogDeregistration
                        {
                            Datacenter = node.Datacenter, // 可省略；默认取客户端配置
                            Node = node.Name,       // 节点名（UI Catalog 页的 Node 列）
                            ServiceID = entry.Service.ID           // 要删除的实例 ID
                        };

                        await _consulClient.Catalog.Deregister(dereg);
                        logger.LogInformation("ServicesDeregisterAsync.Catalog.Deregister 成功 {ID}", entry.Service.ID);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("ServicesDeregisterAsync:{Message}\r\n{StackTrace}", e.Message, e.StackTrace);
                    }

                }
            }
        }

    }
}
