using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// 注册、注销workerId，刷新workerId的有效期
    /// 后台任务
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class WorkerIdWorkerService(ILogger<WorkerIdWorkerService> logger, WorkerIdManager workerIdManager) 
        : BackgroundService
    {
        private readonly ILogger<WorkerIdWorkerService> _logger = logger;

        private readonly WorkerIdManager _workerIdManager = workerIdManager;

        /// <summary>
        /// 注册workerId
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _workerIdManager.RegisterWorkerId();
            await base.StartAsync(cancellationToken);
            //await base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //五秒刷新
                await Task.Delay(5000, stoppingToken);
                _workerIdManager.Refresh();
                _logger.LogDebug(" Refresh WorkId");
            }
        }


        /// <summary>
        /// 注销workerId
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async override Task StopAsync(CancellationToken cancellationToken)
        { 
            await base.StopAsync(cancellationToken);
            //await base.StopAsync(cancellationToken);  
            await _workerIdManager.UnRegister();
        }

    }
}
