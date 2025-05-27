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

namespace QYQ.Base.Consul.Grpc.Serivce
{
    /// <summary>
    /// consul 注册 和 注销
    /// </summary>
    public class ConsulGRPCHostedService(ILogger<ConsulGRPCHostedService> logger,IConsulClient consulClient, IConfiguration configuration) : IHostedService
    {
        private readonly IConsulClient _consulClient = consulClient;

        private readonly string _serviceId = Guid.NewGuid().ToString();

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
            var ipAddress = GetIPAddress();
            int port = agent.Port;
            agent.Meta.Add("Env", configuration["apollo:Env"]);

            // 在启动时移除相同地址和端口的旧服务
            await ServicesDeregisterAsync();

            var registration = new AgentServiceRegistration
            {
                ID = _serviceId,
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
            await _consulClient.Agent.ServiceRegister(registration, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // 在停止时注销服务
            await _consulClient.Agent.ServiceDeregister(_serviceId, CancellationToken.None);

            //await ServicesDeregisterAsync();
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
            var ipAddress = GetIPAddress();
            int port = agent.Port;
            // 在启动时移除相同地址和端口的旧服务
            var servicesList = await _consulClient.Catalog.Service(agent.ServiceName);
            var services = servicesList.Response;
            logger.LogInformation("Consul Grpc服务列表: {services}", JsonConvert.SerializeObject(services));
            foreach (var service in services)
            {
                if (service.ServiceAddress == ipAddress && service.ServicePort == port && service.ServiceName.Equals(agent.ServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation($"Service Name:{service.ServiceName},Service ID: {service.ServiceID}, Address: {service.ServiceAddress}, Port: {service.ServicePort}");
                    await _consulClient.Agent.ServiceDeregister(service.ServiceID);
                }
            }
        }

        /// <summary>
        /// 获取ip地址
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            string ip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                        .Select(p => p.GetIPProperties())
                        .SelectMany(p => p.UnicastAddresses)
                        .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
                        .FirstOrDefault()?.Address.ToString();

            return ip;
        }
    }
}
