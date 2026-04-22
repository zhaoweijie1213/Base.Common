using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QYQ.Base.SnowId.Options;
using System;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// 管理 SnowId 的 WorkerId 注册、续租与注销，确保同一实例在 Redis 中独占槽位。
    /// </summary>
    /// <remarks>
    /// 初始化 <see cref="WorkerIdManager"/>，并读取 Redis Provider 配置。
    /// </remarks>
    /// <param name="logger">日志记录器，用于输出注册与续租过程的运行信息。</param>
    /// <param name="easyCachingProviderFactory">EasyCaching Provider 工厂，用于获取 Redis Provider。</param>
    /// <param name="options">SnowId Redis 配置选项，包含 ProviderName 等参数。</param>
    public class WorkerIdManager(ILogger<WorkerIdManager> logger, IEasyCachingProviderFactory easyCachingProviderFactory, IOptions<SnowIdRedisOptions> options)
    {
        /// <summary>
        /// 记录 WorkerId 生命周期相关日志。
        /// </summary>
        private readonly ILogger<WorkerIdManager> _logger = logger;

        /// <summary>
        /// 当前注册的workerId
        /// </summary>
        private int _workerId { get; set; } = -1;

        /// <summary>
        /// 是否已成功注册 WorkerId
        /// </summary>
        public bool IsRegistered => _workerId >= 0;

        /// <summary>
        /// WorkerId 槽位最大值（包含边界）。
        /// </summary>
        private const int max = 63;

        /// <summary>
        /// 注册 WorkerId 时的最大扫描轮次。
        /// </summary>
        private const int MaxRegisterSweeps = 3;

        /// <summary>
        /// 心跳 Key 过期时间（秒）。
        /// </summary>
        private const int HeartbeatTtlSeconds = 25;

        /// <summary>
        /// 当前实例唯一标识，用于校验心跳 Key 归属。
        /// </summary>
        private static readonly string InstanceIdentity = BuildInstanceIdentity();

        /// <summary>
        /// EasyCaching Provider 工厂。
        /// </summary>
        private readonly IEasyCachingProviderFactory _easyCachingProviderFactory = easyCachingProviderFactory;

        /// <summary>
        /// Redis Provider 名称。
        /// </summary>
        private readonly string _providerName = string.IsNullOrWhiteSpace(options.Value?.ProviderName)
                ? SnowIdRedisOptions.DefaultProviderName
                : options.Value.ProviderName;

        /// <summary>
        /// 获取当前配置对应的 Redis 缓存 Provider。
        /// </summary>
        /// <returns>可用于执行 WorkerId 相关 Redis 操作的 <see cref="IRedisCachingProvider"/>。</returns>
        public IRedisCachingProvider GetRedis()
        {
           return  _easyCachingProviderFactory.GetRedisProvider(_providerName);
        }

        /// <summary>
        /// 尝试注册可用的 WorkerId 槽位，并在成功后写入心跳 Key。
        /// 在 Redis 中为当前进程抢占一个唯一的 WorkerId（0~63），供 SnowId 生成器使用，避免多实例冲突
        /// </summary>
        /// <returns>
        /// 注册成功返回 <c>true</c>；当全部槽位均不可用且重试结束后返回 <c>false</c>。
        /// </returns>
        public async Task<bool> RegisterWorkerId()
        {
            var redis = GetRedis();
            //计算重试等待时间
            var delaySeconds = HeartbeatTtlSeconds / 2;

            _logger.LogInformation("开始注册 WorkerId...");
            /***
             * 每一轮都会随机选一个起点 start（0~63），然后遍历全部 64 个槽位
             * 方法流程（按执行顺序）
                获取 Redis provider：var redis = GetRedis();
                计算重试等待时间：delaySeconds = HeartbeatTtlSeconds / 2
                    当前 HeartbeatTtlSeconds = 25，所以等待约 12 秒。
                最多进行 MaxRegisterSweeps = 3 轮扫描。
                每一轮都会随机选一个起点 start（0~63），然后遍历全部 64 个槽位：
                    计算候选 workerId = (start + i) % 64
                    构造心跳键：{AppDomain}_SnowWorkerHeartbeat:{workerId}
                    执行 Redis SET key value EX 25 NX
                    NX 表示只在 key 不存在时写入，相当于“抢锁”
                    value 是当前实例唯一标识 InstanceIdentity
                一旦某个槽位抢占成功：
                    把 _workerId 设为该值
                    记录成功日志
                    立即返回 true
                若本轮没抢到且还有下一轮：
                    记录 warning 日志
                    Task.Delay 等待后继续下一轮
                三轮都失败：
                    记录 error 日志（64 个槽位都被占用）
                    返回 false
                关键点总结
                    并发安全来源：Redis 的 SET NX + 过期时间 原子语义。
                    防僵死：心跳 key 有 TTL（25 秒），实例挂掉后槽位会自动释放。
                    负载均衡考虑：每轮随机起点，减少多个实例总从 0 开始抢导致的热点冲突。
                返回值语义：
                    true：注册成功，_workerId 已可用
                    false：当前阶段无法拿到任何 WorkerId（通常应视为启动失败）
             * 
             * ***/

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
        /// 生成指定 WorkerId 的心跳 Key。
        /// </summary>
        /// <param name="workerId">WorkerId 槽位编号。</param>
        /// <returns>用于标识该 WorkerId 心跳状态的 Redis Key。</returns>
        public static string GetHeartbeatKey(int workerId)
        {
            return $"{AppDomain.CurrentDomain.FriendlyName}_SnowWorkerHeartbeat:{workerId}";
        }

        private static string BuildInstanceIdentity()
        {
            return $"{Environment.MachineName}:{Environment.ProcessId}:{AppDomain.CurrentDomain.FriendlyName}:{Guid.NewGuid():N}";
        }

        /// <summary>
        /// 获取当前实例已注册的 WorkerId。
        /// </summary>
        /// <returns>当前实例持有的 WorkerId。</returns>
        /// <exception cref="SystemException">当实例尚未注册 WorkerId 时抛出。</exception>
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
        /// 注销当前 WorkerId，并在归属校验通过后删除对应心跳 Key。
        /// </summary>
        /// <returns>表示异步注销流程的任务。</returns>
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
        /// 刷新当前 WorkerId 的心跳有效期，必要时尝试重新抢占同一槽位。
        /// </summary>
        /// <returns>
        /// 刷新或重新抢占成功返回 <c>true</c>；若丢失归属、未注册或发生异常则返回 <c>false</c>。
        /// </returns>
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
                    //_workerId = -1;
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
