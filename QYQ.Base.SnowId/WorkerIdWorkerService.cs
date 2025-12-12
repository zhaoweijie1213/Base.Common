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
            var failCount = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, stoppingToken);

                    _logger.LogDebug("刷新 WorkerId 的有效期...");
                    await _workerIdManager.Refresh();

                    failCount = 0; // 成功就清零
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // 正常停止
                    break;
                }
                catch (StackExchange.Redis.RedisTimeoutException ex)
                {
                    failCount++;
                    _logger.LogError(ex, "刷新 WorkerId 超时（第 {FailCount} 次）", failCount);
                    await BackoffDelayAsync(failCount, stoppingToken);
                }
                catch (StackExchange.Redis.RedisConnectionException ex)
                {
                    failCount++;
                    _logger.LogError(ex, "刷新 WorkerId 连接异常（第 {FailCount} 次）", failCount);
                    await BackoffDelayAsync(failCount, stoppingToken);
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(ex, "刷新 WorkerId 未知异常（第 {FailCount} 次）", failCount);
                    await BackoffDelayAsync(failCount, stoppingToken);
                }
            }
            _logger.LogInformation("WorkerId 任务停止.");
        }

        private static Task BackoffDelayAsync(int failCount, CancellationToken token)
        {
            // 1s, 2s, 4s, 8s... 最大 30s
            var seconds = Math.Min(30, (int)Math.Pow(2, Math.Min(failCount, 5)));
            return Task.Delay(TimeSpan.FromSeconds(seconds), token);
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
