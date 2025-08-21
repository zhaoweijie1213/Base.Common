using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QYQ.Base.SnowId.Interface;
using QYQ.Base.SnowId.Options;
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
        public SnowIdRedisGenerator(ILogger<SnowIdRedisGenerator> logger, WorkerIdManager workerIdManager, IConfiguration configuration, IOptions<SnowIdOptions> options)
            : base(options)
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

        /// <summary>
        /// 根据指定时间生成id
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="isMillisecondPrecision"></param>
        /// <returns></returns>
        public override long CreateId(DateTime dateTime, bool isMillisecondPrecision = true)
        {
            int workerId = _workerIdManager.GetWorkerId();
            return base.CreateId(workerId, dateTime, isMillisecondPrecision);
        }
    }
}