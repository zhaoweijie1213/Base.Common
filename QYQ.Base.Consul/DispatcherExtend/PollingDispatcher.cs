using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
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
    /// <param name="consulClientOption"></param>
    public class PollingDispatcher(IOptionsMonitor<ConsulServiceOptions> consulClientOption) : AbstractConsulDispatcher(consulClientOption)
    {
        /// <summary>
        /// 索引
        /// </summary>
        private readonly ConcurrentDictionary<string, int> _serviveIndex = new();

        /// <summary>
        /// 轮询
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        protected override int GetIndex(string serviceName)
        {
            //int index = _serviveIndex.GetOrAdd(serviceName, 0);
            //index = Interlocked.Increment(ref index) % _agentServices[serviceName].Length;
            //_serviveIndex[serviceName] = index;
            ////if (index == int.MaxValue)
            ////{
            ////    Interlocked.Exchange(ref _iTotalCount, 0);
            ////}
            //return index;

            return _serviveIndex.AddOrUpdate(serviceName, 0, (key, value) =>
            {
                int newValue = (value + 1) % _agentServices[serviceName].Length;
                return newValue;
            });
        }
    }
}
