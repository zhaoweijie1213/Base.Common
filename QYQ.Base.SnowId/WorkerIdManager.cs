using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        private readonly IEasyCachingProviderFactory _easyCachingProviderFactory;



        /// <summary>
        /// 
        /// </summary>
        public WorkerIdManager(ILogger<WorkerIdManager> logger,IEasyCachingProviderFactory easyCachingProviderFactory)
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
        public async Task RegisterWorkerId()
        {
            var redis = GetRedis();

            _logger.LogInformation("开始注册 WorkerId...");
            //获取redis Key
            string key = GetWorkerIdKey();

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
                //将workerId放入已使用集合
                var res = await redis.SAddAsync(GetUsageIdKey(), new List<long>() { workerId });
                if (res > 0)
                {
                    _workerId = Convert.ToInt32(workerId);
                    _logger.LogInformation("WorkerId 注册成功，workerId: {workerId}", _workerId);
                    break;
                }
            }
   


            //return _workerId;
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

            //_workerId = -1;
            _logger.LogInformation("WorkerId 注销成功");
        }


        /// <summary>
        /// 刷新有效期
        /// </summary>
        /// <returns></returns>
        public void Refresh()
        {
            if (_workerId != -1)
            {
                var redis = GetRedis();
                //显示当前worker Id
                redis.KeyExpire(GetUsageIdKey(), 15);
                _logger.LogDebug("刷新 WorkerId 的有效期，workerId: {workerId}", _workerId);
            }

        }

    }
}
