using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.DispatcherExtend
{
    /// <summary>
    /// 服务健康维护
    /// </summary>
    public class AgentServiceHealthCheck : BackgroundService
    {

        private readonly AbstractConsulDispatcher _abstractConsulDispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abstractConsulDispatcher"></param>
        public AgentServiceHealthCheck(AbstractConsulDispatcher abstractConsulDispatcher)
        {
            _abstractConsulDispatcher = abstractConsulDispatcher;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(((int)TimeSpan.FromSeconds(30).TotalMilliseconds), stoppingToken);
                await _abstractConsulDispatcher.CheckHealthService();
            }
        }

    }
}
