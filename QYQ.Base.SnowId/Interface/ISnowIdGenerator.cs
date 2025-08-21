using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId.Interface
{
    /// <summary>
    /// 雪花id生成
    /// </summary>
    public interface ISnowIdGenerator
    {
        /// <summary>
        /// 生成id
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public long CreateId(int serverId);

        /// <summary>
        /// 生成id
        /// </summary>
        /// <returns></returns>
        public long CreateId();

        /// <summary>
        /// 生成一个指定时间的 Id（时间用于构造时间差部分）
        /// </summary>
        /// <param name="dateTime">用于生成ID的时间（建议UTC或能正确转换为UTC的本地时间）</param>
        /// <param name="isMillisecondPrecision">是否是毫秒级的时间</param>
        /// <returns></returns>
        public long CreateId(DateTime dateTime, bool isMillisecondPrecision = true);


        /// <summary>
        /// 根据时间生成id
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="dateTime">用于生成ID的时间（建议UTC或能正确转换为UTC的本地时间</param>
        /// <param name="isMillisecondPrecision">是否是毫秒级的时间</param>
        /// <returns></returns>
        public long CreateId(int workId, DateTime dateTime, bool isMillisecondPrecision = true);


        /// <summary>
        /// 获取雪花Id
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long GetSnowId(DateTime time);


        /// <summary>
        /// sonwId转换时间
        /// </summary>
        /// <param name="sonwId"></param>
        /// <returns></returns>
        public DateTime GetDateTime(long sonwId);

    }
}
