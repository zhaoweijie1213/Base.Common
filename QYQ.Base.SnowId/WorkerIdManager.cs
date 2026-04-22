using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QYQ.Base.SnowId.Options;
using System;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// workerId管理
    /// </summary>
    public class WorkerIdManager
    {
        private readonly ILogger<WorkerIdManager> _logger;

        /// <summary>
        /// 当前注册的workerId
        /// </summary>
        private int _workerId { get; set; } = -1;

        /// <summary>
        /// 是否已成功注册 WorkerId
        /// </summary>
        public bool IsRegistered => _workerId >= 0;

        private const int max = 63;
        private const int MaxRegisterSweeps = 3;

        private const int HeartbeatTtlSeconds = 25;
        private static readonly string InstanceIdentity = BuildInstanceIdentity();

        private readonly IEasyCachingProviderFactory _easyCachingProviderFactory;
        private readonly string _providerName;



        /// <summary>
        /// 
        /// </summary>
        public WorkerIdManager(ILogger<WorkerIdManager> logger, IEasyCachingProviderFactory easyCachingProviderFactory, IOptions<SnowIdRedisOptions> options)
        {
            _logger = logger;
            _easyCachingProviderFactory = easyCachingProviderFactory;
            _providerName = string.IsNullOrWhiteSpace(options.Value?.ProviderName)
                ? SnowIdRedisOptions.DefaultProviderName
                : options.Value.ProviderName;
        }

        /// <summary>
        /// 获取redis
        /// </summary>
        /// <returns></returns>
        public IRedisCachingProvider GetRedis()
        {
           return  _easyCachingProviderFactory.GetRedisProvider(_providerName);
        }

        /// <summary>
        /// 注册workerId
        /// </summary>
        public async Task<bool> RegisterWorkerId()
        {
            var redis = GetRedis();
            var delaySeconds = HeartbeatTtlSeconds / 2;

            _logger.LogInformation("开始注册 WorkerId...");
            for (var sweep = 1; sweep <= MaxRegisterSweeps; sweep++)
            {
                var start = Random.Shared.Next(0, max + 1);
                for (var i = 0; i <= max; i++)
                {
                    var workerId = (start + i) % (max + 1);
                    var heartbeatKey = GetHeartbeatKey(workerId);
                    var acquired = await redis.StringSetAsync(
                        heartbeatKey,
                        InstanceIdentity,
                        TimeSpan.FromSeconds(HeartbeatTtlSeconds),
                        "nx");
                    if (!acquired) continue;

                    _workerId = workerId;
                    _logger.LogInformation("WorkerId 注册成功，workerId: {workerId}，实例标识: {instanceIdentity}", _workerId, InstanceIdentity);
                    return true;
                }

                if (sweep < MaxRegisterSweeps)
                {
                    _logger.LogWarning("WorkerId 注册第 {sweep} 轮未抢到槽位，等待 {delay}s 后重试。", sweep, delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            _logger.LogError("WorkerId 注册失败：全部 {slotCount} 个槽位均被占用，启动中止。", max + 1);
            return false;
        }

        /// <summary>
        /// 获取心跳 key
        /// </summary>
        /// <param name="workerId"></param>
        /// <returns></returns>
        public static string GetHeartbeatKey(int workerId)
        {
            return $"{AppDomain.CurrentDomain.FriendlyName}_SnowWorkerHeartbeat:{workerId}";
        }

        private static string BuildInstanceIdentity()
        {
            return $"{Environment.MachineName}:{Environment.ProcessId}:{AppDomain.CurrentDomain.FriendlyName}:{Guid.NewGuid():N}";
        }

        /// <summary>
        /// 获取workerid
        /// </summary>
        /// <returns></returns>
        public int GetWorkerId()
        {
            if (_workerId < 0)
            {
                _logger.LogError("WorkerId 未注册");
                throw new SystemException("请先注册workerId!");
            }
            _logger.LogDebug("当前注册的 WorkerId: {workerId}", _workerId);
            return _workerId;
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public async Task UnRegister()
        {
            _logger.LogInformation("注销 WorkerId: {workerId}", _workerId);
            if (_workerId < 0)
            {
                _logger.LogInformation("尚未注册 WorkerId，无需注销。");
                return;
            }

            var redis = GetRedis();
            var heartbeatKey = GetHeartbeatKey(_workerId);
            var owner = await redis.StringGetAsync(heartbeatKey);

            if (owner == InstanceIdentity)
            {
                await redis.KeyDelAsync(heartbeatKey);
            }
            else
            {
                _logger.LogWarning("WorkerId {workerId} 当前归属为 {owner}，跳过删除心跳。", _workerId, owner);
            }

            //_workerId = -1;
            _logger.LogInformation("WorkerId 注销成功");
        }


        /// <summary>
        /// 刷新有效期
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshAsync()
        {
            if (_workerId != -1)
            {
                try
                {
                    var redis = GetRedis();
                    var heartbeatKey = GetHeartbeatKey(_workerId);
                    var owner = await redis.StringGetAsync(heartbeatKey);

                    if (owner == InstanceIdentity)
                    {
                        return await redis.KeyExpireAsync(heartbeatKey, HeartbeatTtlSeconds);
                    }

                    if (string.IsNullOrWhiteSpace(owner))
                    {
                        var reacquired = await redis.StringSetAsync(
                            heartbeatKey,
                            InstanceIdentity,
                            TimeSpan.FromSeconds(HeartbeatTtlSeconds),
                            "nx");
                        if (reacquired)
                        {
                            return true;
                        }

                        owner = await redis.StringGetAsync(heartbeatKey);
                    }

                    _logger.LogError("WorkerId {workerId} 已被其他实例 {owner} 占用，放弃该 slot。", _workerId, owner);
                    _workerId = -1;
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "刷新 WorkerId 的有效期时出现异常，workerId: {workerId}", _workerId);
                    return false;
                }
            }
            _logger.LogWarning("刷新 WorkerId 有效期失败，尚未注册 WorkerId。");
            return false;
        }

    }
}
