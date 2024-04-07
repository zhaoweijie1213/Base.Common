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
