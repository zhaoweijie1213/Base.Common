using Consul;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.Consul
{
    /// <summary>
    /// conusl所有代理注册
    /// </summary>
    [Obsolete("使用各自的注册类型的HostedService完成注册和注销")]
    public class AgentServiceRegistrationQYQHostedService : IHostedService
    {
        private readonly IConsulClient _consulClient;
        private readonly IEnumerable<AgentServiceRegistration> _servicesRegistration;

        /// <summary>
        /// 
        /// </summary>
        public AgentServiceRegistrationQYQHostedService(
            IConsulClient consulClient,
            IEnumerable<AgentServiceRegistration> servicesRegistration)
        {
            _consulClient = consulClient;
            _servicesRegistration = servicesRegistration;
        }

        /// <summary>
        /// 开始注册
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var serviceRegistration in _servicesRegistration)
            {
                await _consulClient.Agent.ServiceRegister(serviceRegistration, cancellationToken);
            }
        }

        /// <summary>
        /// 注销注册
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var serviceRegistration in _servicesRegistration)
            {
                await _consulClient.Agent.ServiceDeregister(serviceRegistration.ID, cancellationToken);
            }
        }
    }
}
