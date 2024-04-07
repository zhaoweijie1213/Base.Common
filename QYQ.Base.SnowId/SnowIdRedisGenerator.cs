using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QYQ.Base.SnowId.Interface;
using System.Collections.Concurrent;
using Yitter.IdGenerator;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// 雪花id生成
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class SnowIdRedisGenerator: SnowIdBaseGenerator, ISnowIdGenerator
    {

        private readonly WorkerIdManager _workerIdManager;

        /// <summary>
        /// 
        /// </summary>
        public SnowIdRedisGenerator(ILogger<SnowIdRedisGenerator> logger, WorkerIdManager workerIdManager, IConfiguration configuration)
        {
            _workerIdManager = workerIdManager;
            DataCenterId = configuration.GetSection("DataCenterId").Get<uint?>() ?? Convert.ToUInt32(new Random().Next(1, 20));
            logger.LogInformation("Init DataCenterId:{DataCenterId}", DataCenterId);
        }



        /// <summary>
        /// 生成id
        /// 启用workerId注册时使用
        /// </summary>
        /// <returns></returns>
        public override long CreateId()
        {
            int workerId = _workerIdManager.GetWorkerId();
            var idGenerator = GetIIdGenerator(workerId);
            return idGenerator.NewLong();
        }
    }
}