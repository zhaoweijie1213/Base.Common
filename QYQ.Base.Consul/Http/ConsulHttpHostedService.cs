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
            var ipAddress = GetIPAddress();
            int port = agent.Port;
            agent.Meta.Add("Env", configuration["apollo:Env"]);
            // 在启动时移除相同地址和端口的旧服务
            var servicesList = await _consulClient.Agent.Services(cancellationToken);
            var list = servicesList.Response.Where(i => i.Value.Service == agent.ServiceName && i.Value.Address == ipAddress && i.Value.Port == port).Select(i => i.Value).ToList();
            logger.LogInformation("StartAsync.查询相同Consul服务列表:{list}", JsonConvert.SerializeObject(list));
            foreach (var service in list)
            {
                await _consulClient.Agent.ServiceDeregister(service.ID, cancellationToken);
                logger.LogInformation("Deregistered existing service: {ServiceId}", service.ID);
            }

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
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // 在停止时注销服务
            await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
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
