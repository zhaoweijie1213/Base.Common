using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        private const int max = 63;

        private const int HeartbeatTtlSeconds = 25;
        private static readonly TimeSpan UsageKeyExpire = TimeSpan.FromSeconds(30);

        private readonly IEasyCachingProviderFactory _easyCachingProviderFactory;



        /// <summary>
        /// 
        /// </summary>
        public WorkerIdManager(ILogger<WorkerIdManager> logger, IEasyCachingProviderFactory easyCachingProviderFactory)
        {
            _logger = logger;
            _easyCachingProviderFactory = easyCachingProviderFactory;
        }

        /// <summary>
        /// 获取redis
        /// </summary>
        /// <returns></returns>
        public IRedisCachingProvider GetRedis()
        {
           return  _easyCachingProviderFactory.GetRedisProvider("SnowIdRedis");
        }

        /// <summary>
        /// 注册workerId
        /// </summary>
        public async Task<bool> RegisterWorkerId()
        {
            var redis = GetRedis();

            _logger.LogInformation("开始注册 WorkerId...");
            //获取redis Key
            string key = GetWorkerIdKey();
            string usageKey = GetUsageIdKey();
            string instanceIdentity = GetInstanceIdentity();

            while (true)
            {
                long workerId = await redis.IncrByAsync(key);
                //判断workid是否大于最大值
                if (workerId > max)
                {
                    //将值初始化
                    bool status = await redis.StringSetAsync(key, "0");
                    if (status) workerId = 0;
                }
                var heartbeatKey = GetHeartbeatKey(Convert.ToInt32(workerId));
                var heartbeatExists = await redis.KeyExistsAsync(heartbeatKey);
                if (heartbeatExists)
                {
                    _logger.LogWarning("WorkerId {workerId} 已存在心跳，跳过当前候选。", workerId);
                    continue;
                }

                //将workerId放入已使用集合
                var res = await redis.SAddAsync(usageKey, new List<long>() { workerId });
                if (res > 0)
                {
                    var heartbeatSet = await redis.StringSetAsync(heartbeatKey, instanceIdentity, TimeSpan.FromSeconds(HeartbeatTtlSeconds));
                    if (!heartbeatSet)
                    {
                        _logger.LogError("WorkerId {workerId} 心跳写入失败，停止注册。", workerId);
                        return false;
                    }

                    _workerId = Convert.ToInt32(workerId);
                    _logger.LogInformation("WorkerId 注册成功，workerId: {workerId}，实例标识: {instanceIdentity}", _workerId, instanceIdentity);
                    return true;
                }
                _logger.LogWarning("WorkerId {workerId} 抢占失败，可能已被其他实例占用。", workerId);
            }
        }

        /// <summary>
        /// workerId的key
        /// </summary>
        /// <returns></returns>
        public static string GetWorkerIdKey()
        {
            return $"{AppDomain.CurrentDomain.FriendlyName}_SnowWorkerIds";
        }

        /// <summary>
        /// 获取已使用id的key
        /// </summary>
        /// <returns></returns>
        public static string GetUsageIdKey()
        {
            return $"{AppDomain.CurrentDomain.FriendlyName}_SnowUsageIds";
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

        private static string GetInstanceIdentity()
        {
            return $"{Environment.MachineName}:{Process.GetCurrentProcess().Id}:{AppDomain.CurrentDomain.FriendlyName}";
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
            var redis = GetRedis();

            //从已使用集合移除id
            await redis.SRemAsync(GetUsageIdKey(), new List<long>() { _workerId });
            await redis.KeyDelAsync(GetHeartbeatKey(_workerId));

            _workerId = -1;
            _logger.LogInformation("WorkerId 注销成功");
        }


        /// <summary>
        /// 刷新有效期
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Refresh()
        {
            if (_workerId != -1)
            {
                try
                {
                    var redis = GetRedis();
                    var usageRenewed = await redis.KeyExpireAsync(GetUsageIdKey(), (int)UsageKeyExpire.TotalSeconds);
                    var heartbeatRenewed = await redis.KeyExpireAsync(GetHeartbeatKey(_workerId), HeartbeatTtlSeconds);

                    if (!usageRenewed)
                    {
                        _logger.LogWarning("刷新 WorkerId 集合过期时间失败，workerId: {workerId}", _workerId);
                    }

                    if (!heartbeatRenewed)
                    {
                        _logger.LogWarning("刷新 WorkerId 心跳失败，workerId: {workerId}", _workerId);
                    }

                    _logger.LogDebug("刷新 WorkerId 的有效期，workerId: {workerId}", _workerId);
                    return usageRenewed && heartbeatRenewed;
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
