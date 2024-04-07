using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.DispatcherExtend
{
    /// <summary>
    /// 平均
    /// </summary>
    public class AverageDispatcher : AbstractConsulDispatcher
    {
        /// <summary>
        /// 随机数
        /// </summary>
        private static readonly Random random = new();

        /// <summary>
        /// 用于线程安全的锁对象
        /// </summary>
        private static readonly object syncLock = new();

        #region Identity
        //private static int _iTotalCount = 0;
        //private static int iTotalCount
        //{
        //    get
        //    {
        //        return _iTotalCount;
        //    }
        //    set
        //    {
        //        _iTotalCount = value >= Int32.MaxValue ? 0 : value;
        //    }
        //}

        //private ConsulServiceOptions _ConsulClientOption = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consulClientOption"></param>
        public AverageDispatcher(IOptionsMonitor<ConsulServiceOptions> consulClientOption)
            : base(consulClientOption)
        {

        }
        #endregion

        /// <summary>
        /// 平均
        /// </summary>
        /// <returns></returns>
        protected override int GetIndex(int length)
        {
            lock (syncLock) // 确保线程安全
            {
                return random.Next(0, length); // 生成 0 到 length 之间的随机数
            }
            //return new Random(iTotalCount++).Next(0, length);
        }
    }
}
