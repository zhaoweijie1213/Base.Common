using Consul;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.DispatcherExtend
{
    /// <summary>
    /// 权重
    /// </summary>
    public class WeightDispatcher : AbstractConsulDispatcher
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
                _iTotalCount = value >= Int32.MaxValue ? 0 : value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consulClientOption"></param>
        public WeightDispatcher(IOptionsMonitor<ConsulServiceOptions> consulClientOption) : base(consulClientOption)
        {

        }
        #endregion

        /// <summary>
        /// 按权重选择地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public override string ChooseAddress(string serviceName)
        {
            base.InitAgentServiceDictionary(serviceName);
            AgentService agentService = null;
            var serviceDictionaryNew = new List<AgentService>();
            if (!_agentServices.TryGetValue(serviceName, out AgentService[] services))
            {
                var agents = services.ToList();
                foreach (var service in agents)
                {
                    serviceDictionaryNew.AddRange(Enumerable.Repeat(service, int.TryParse(service.Meta?["weight"], out int iWeight) ? 1 : iWeight));
                }
                int index = new Random(DateTime.Now.Millisecond).Next(0, int.MaxValue) % serviceDictionaryNew.Count;
                agentService = serviceDictionaryNew[index];

                return $"{agentService.Address}:{agentService.Port}";
            }
            else
            {
                return "";
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override int GetIndex(int length)
        {
            throw new NotImplementedException();
        }
    }
}
