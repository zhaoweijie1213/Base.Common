using Consul;
using Google.Protobuf.Reflection;
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

namespace QYQ.Base.Consul.Http
{
    /// <summary>
    /// consul 注册 和 注销
    /// </summary>
    public class ConsulHttpHostedService(ILogger<ConsulHttpHostedService> logger,IConsulClient consulClient, IConfiguration configuration) : IHostedService
    {

        private readonly IConsulClient _consulClient = consulClient;

        private readonly string _serviceId = Guid.NewGuid().ToString();

        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var _serviceOptions = configuration.GetSection("ConsulOptions").Get<ConsulServiceOptions>();
            var agent = _serviceOptions.ConsulAgents.FirstOrDefault(i => i.AgentCategory == AgentCategory.HTTP) ?? throw new ArgumentException("Consul agent configuration not found");
            var ipAddress = NetworkUtil.GetHostIPv4(_serviceOptions.HostIPAddress);
            logger.LogInformation("StartAsync:Consul注册当前IP地址{ipAddress}", ipAddress);
            int port = agent.Port;
            agent.Meta.Add("Env", configuration["apollo:Env"]);

            //移除重复注册的旧服务
            //await ServicesDeregisterAsync();

            var registration = new AgentServiceRegistration
            {
                ID = _serviceId,                      // 唯一标识，可用“服务名+实例IP+端口”等方式保证全局唯一
                Name = agent.ServiceName,             // 在 Consul 中注册的服务名称，一般与逻辑服务名保持一致
                Address = ipAddress,                  // 服务实例的IP（客户端调用时会用到）
                Port = port,                          // 服务实例监听的端口
                Tags = agent.Tags,                    // 可选：用来给服务打标签，便于按环境、版本或区域筛选
                Meta = agent.Meta,                    // 可选：自定义元数据，比如版本号、权重等
                Check = new AgentServiceCheck
                {
                    // 注册超时
                    Timeout = TimeSpan.FromSeconds(5),
                    // 服务停止多久后注销服务
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    // 健康检查地址
                    HTTP = $"http://{ipAddress}:{port}/api/Health",
                    // 健康检查时间间隔
                    Interval = TimeSpan.FromSeconds(10),
                }
            };

            //注册服务
            await _consulClient.Agent.ServiceRegister(registration, CancellationToken.None);
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
                await _consulClient.Agent.ServiceDeregister(_serviceId, CancellationToken.None);
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
            var agent = _serviceOptions.ConsulAgents.FirstOrDefault(i => i.AgentCategory == AgentCategory.HTTP) ?? throw new ArgumentException("Consul agent configuration not found");
            var ipAddress = NetworkUtil.GetHostIPv4(_serviceOptions.HostIPAddress);
            logger.LogInformation("ServicesDeregisterAsync:Consul注册当前IP地址{ipAddress}", ipAddress);
            int port = agent.Port;
            // 移除相同地址和端口的旧服务
            var healthResult = await _consulClient.Health.Service(agent.ServiceName, tag: "", passingOnly: true);
            var healthyInstances = healthResult.Response;
            logger.LogInformation("Consul Grpc服务列表: {services}", JsonConvert.SerializeObject(healthyInstances));
            foreach (var entry in healthyInstances)
            {
                //var service = entry.Service;
                if (entry.Service.Address == ipAddress && entry.Service.Port == port && entry.Service.Service.Equals(agent.ServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation($"Service Name:{entry.Service.Service},Service ID: {entry.Service.ID}, Address: {entry.Service.Address}, Port: {entry.Service.Port}");
                    await _consulClient.Agent.ServiceDeregister(entry.Service.ID);
                }
            }
        }


    }
}
