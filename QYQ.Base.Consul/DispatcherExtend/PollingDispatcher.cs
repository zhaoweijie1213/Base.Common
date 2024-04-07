using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.DispatcherExtend
{
    /// <summary>
    /// 轮询
    /// </summary>
    public class PollingDispatcher : AbstractConsulDispatcher
    {
        #region Identity
        private static int _iTotalCount = 0;
        private static int iTotalCount
        {
            get
            {
                return _iTotalCount;
            }
            set
            {
                _iTotalCount = value >= int.MaxValue ? 0 : value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consulClientOption"></param>
        public PollingDispatcher(IOptionsMonitor<ConsulServiceOptions> consulClientOption) : base(consulClientOption)
        {
        }
        #endregion

        /// <summary>
        /// 轮询
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override int GetIndex(int length)
        {
            int index = Interlocked.Increment(ref _iTotalCount);
            if (index == int.MaxValue)
            {
                Interlocked.Exchange(ref _iTotalCount, 0);
            }
            return index % length;
        }
    }
}
