using Microsoft.Extensions.Options;
using QYQ.Base.SnowId.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace QYQ.Base.SnowId
{
    /// <summary>
    /// 雪花Id生成基类
    /// </summary>
    public class SnowIdBaseGenerator
    {
        /// <summary>
        /// 雪花Id生成基类
        /// </summary>
        /// <param name="options"></param>
        public SnowIdBaseGenerator(IOptions<SnowIdOptions> options)
        {
            if (options.Value.BaseTime == null) 
            {
                BaseTime = new DateTime(2020, 2, 20, 2, 20, 2, 20, DateTimeKind.Utc);

            }
            else
            {
                BaseTime = options.Value.BaseTime.Value;
            }
        }

        /// <summary>
        /// 基础时间(utc时间)
        /// </summary>
        public DateTime BaseTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public readonly ConcurrentDictionary<int, IIdGenerator> _IdGeneratorsDic = new();

        /// <summary>
        /// workId的长度
        /// </summary>
        public const byte workerIdBitLength = 6;

        /// <summary>
        /// SeqBitLength，序列数位长，默认值6，取值范围 [3, 21]（建议不小于4），决定每毫秒基础生成的ID个数。
        /// 规则要求：DataCenterIdBitLength + WorkerIdBitLength + SeqBitLength 不超过 22。
        /// </summary>
        private const byte seqBitLength = 14;

        /// <summary>
        /// 数据中心ID长度
        /// </summary>

        private const byte dataCenterIdBitLength = 2;

        private readonly Random _random = new();

        /// <summary>
        /// 数据中心Id
        /// </summary>
        protected uint DataCenterId { get; set; }


        /// <summary>
        /// 创建雪花id生成器
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        private IIdGenerator CreateIIdGenerator(int serverId)
        {
            IIdGenerator idGenerator;
            var options = new IdGeneratorOptions()
            {
                WorkerId = Convert.ToUInt16(serverId),
                WorkerIdBitLength = workerIdBitLength,
                SeqBitLength = seqBitLength,
                BaseTime = BaseTime,
                DataCenterIdBitLength = dataCenterIdBitLength,
                DataCenterId = DataCenterId,
            };
            idGenerator = new DefaultIdGenerator(options);
            _IdGeneratorsDic.TryAdd(serverId, idGenerator);
            return idGenerator;
        }


        /// <summary>
        /// 生成id
        /// 启用workerId注册时使用
        /// </summary>
        /// <returns></returns>
        public virtual long CreateId()
        {
            var workerId = _random.Next(1, 50);
            return CreateId(workerId);
        }

        /// <summary>
        /// 生成id
        /// 未启用workerId注册需要自己传入workId
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public virtual long CreateId(int serverId)
        {
            var idGenerator = GetIIdGenerator(serverId);
            return idGenerator.NewLong();
        }


        /// <summary>
        /// 根据时间生成id
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="isMillisecondPrecision"></param>
        /// <returns></returns>
        public virtual long CreateId(DateTime dateTime, bool isMillisecondPrecision = true)
        {
            var workerId = _random.Next(1, 50);
            return CreateId(workerId, dateTime, isMillisecondPrecision);
        }

        /// <summary>
        /// 根据时间生成id
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="dateTime"></param>
        /// <param name="isMillisecondPrecision"></param>
        /// <returns></returns>
        public virtual long CreateId(int workId, DateTime dateTime, bool isMillisecondPrecision = true)
        {
            var idGenerator = GetIIdGenerator(workId);
            return idGenerator.NextId(dateTime, isMillisecondPrecision);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workId"></param>
        /// <returns></returns>
        public IIdGenerator GetIIdGenerator(int workId)
        {
            //if (!_IdGeneratorsDic.TryGetValue(serverId, out var idGenerator))
            //{
            //    idGenerator = CreateIIdGenerator(serverId);
            //}

            var idGenerator = _IdGeneratorsDic.GetOrAdd(workId, CreateIIdGenerator);
            return idGenerator;

        }

        /// <summary>
        /// 获取雪花Id
        /// 时间戳向左偏移22位
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual long GetSnowId(DateTime time)
        {
            return (long)(time - BaseTime).TotalMilliseconds << (workerIdBitLength + seqBitLength + dataCenterIdBitLength);
        }

        /// <summary>
        /// sonwId转换时间
        /// </summary>
        /// <param name="sonwId"></param>
        /// <returns></returns>
        public virtual DateTime GetDateTime(long sonwId)
        {
            long s = sonwId >> (workerIdBitLength + seqBitLength + dataCenterIdBitLength);
            return BaseTime.AddMilliseconds(s);
        }

    }
}
