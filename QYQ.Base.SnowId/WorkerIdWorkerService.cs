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
            try
            {
                await _workerIdManager.RegisterWorkerId();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册 WorkerId 时发生错误");
            }
    
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
            _logger.LogInformation("WorkerId 任务开始执行...");
            while (!stoppingToken.IsCancellationRequested)
            {
                //五秒刷新
                await Task.Delay(5000, stoppingToken);
                _logger.LogDebug("刷新 WorkerId 的有效期...");
                _workerIdManager.Refresh();
            }
            _logger.LogInformation("WorkerId 任务停止.");
        }


        /// <summary>
        /// 注销workerId
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("注销 WorkerId...");
            try
            {
                await _workerIdManager.UnRegister();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注销 WorkerId 时发生错误");
            }
            await base.StopAsync(cancellationToken);
        }

    }
}
