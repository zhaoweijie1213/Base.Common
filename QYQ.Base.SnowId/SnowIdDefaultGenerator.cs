using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QYQ.Base.SnowId.Interface;
using QYQ.Base.SnowId.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// 
    /// </summary>
    public class SnowIdDefaultGenerator : SnowIdBaseGenerator, ISnowIdGenerator
    {
        //private readonly ILogger<SnowIdDefaultGenerator> _logger;

        private readonly int workerId;

        /// <summary>
        /// 
        /// </summary>
        public SnowIdDefaultGenerator(ILogger<SnowIdDefaultGenerator> logger, IConfiguration configuration, IOptions<SnowIdOptions> options)
            : base(options)
        {
            workerId = configuration.GetSection("SnowServerId").Get<int?>() ?? new Random().Next(1, 20);
            DataCenterId = configuration.GetSection("DataCenterId").Get<uint?>() ?? Convert.ToUInt32(new Random().Next(1, 20));
            logger.LogInformation("Init WorkerId:{workerId}", workerId);
            logger.LogInformation("Init DataCenterId:{DataCenterId}", DataCenterId);
        }

        /// <summary>
        /// 创建雪花Id
        /// </summary>
        /// <returns></returns>
        public override long CreateId()
        {
            return base.CreateId(workerId);
        }

        /// <summary>
        /// 根据指定时间创建雪花Id
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="isMillisecondPrecision"></param>
        /// <returns></returns>
        public override long CreateId(DateTime dateTime, bool isMillisecondPrecision = true)
        {
            return base.CreateId(workerId, dateTime, isMillisecondPrecision);
        }
    }
}
