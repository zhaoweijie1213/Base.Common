using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class AgentServiceHealthCheck(ILogger<AgentServiceHealthCheck> logger,IOptions<ConsulDispatcherOptions> consulDispatcherOptions, AbstractConsulDispatcher consulDispatcher) : BackgroundService
    {
        /// <summary>
        /// 定时更新健康服务地址
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(30000, stoppingToken);
                    await consulDispatcher.CheckHealthService();
                    //switch (consulDispatcherOptions.Value.ConsulDispatcherType)
                    //{
                    //    case ConsulDispatcherType.Average:
                    //        var averageDispatcher = serviceProvider.GetService<AverageDispatcher>();
                    //        await averageDispatcher.CheckHealthService();
                    //        break;
                    //    case ConsulDispatcherType.Polling:
                    //        var pollingDispatcher = serviceProvider.GetService<PollingDispatcher>();
                    //        await pollingDispatcher.CheckHealthService();
                    //        break;
                    //    case ConsulDispatcherType.Weight:
                    //        var weightDispatcher = serviceProvider.GetService<WeightDispatcher>();
                    //        await weightDispatcher.CheckHealthService();
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                catch (Exception e)
                {
                    logger.LogError("ExecuteAsync:{Message}\r\n{StackTrace}", e.Message,e.StackTrace);
                }
    
            }
        }

    }
}
