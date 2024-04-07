using Consul;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Consul.DispatcherExtend
{
    /// <summary>
    /// 抽象的Dispatcher基类
    /// 
    /// 模板方法设计模式
    /// </summary>
    public abstract class AbstractConsulDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        protected ConsulServiceOptions _ConsulClientOption = null;

        /// <summary>
        /// 客户端
        /// </summary>
        public ConcurrentDictionary<string, AgentService[]> _agentServices { get; protected set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public AbstractConsulDispatcher(IOptionsMonitor<ConsulServiceOptions> options)
        {
            this._ConsulClientOption = options.CurrentValue;
        }

        /// <summary>
        /// 负载均衡获取地址
        /// </summary>
        /// <param name="mappingUrl">Consul映射后的地址</param>
        /// <returns></returns>
        public string MapAddress(string mappingUrl)
        {
            Uri uri = new(mappingUrl);
            string serviceName = uri.Host;
            string addressPort = this.ChooseAddress(serviceName);
            return $"{uri.Scheme}://{addressPort}{uri.PathAndQuery}";
        }

        /// <summary>
        /// 根据服务名字来获取地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public virtual string ChooseAddress(string serviceName)
        {
            InitAgentServiceDictionary(serviceName);

            if (_agentServices.TryGetValue(serviceName,out AgentService[] agentServices))
            {
                int index = GetIndex(agentServices.Length);
                AgentService agentService = agentServices[index];
                return $"{agentService.Address}:{agentService.Port}";
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 跟Consul交互，获取清单
        /// </summary>
        /// <param name="serviceName"></param>
        protected void InitAgentServiceDictionary(string serviceName)
        {
            
            if (!_agentServices.TryGetValue(serviceName, out AgentService[] services))
            {
                using ConsulClient client = new(c =>
                {
                    c.Address = new Uri(_ConsulClientOption.ConsulAddress);
                    c.Token = _ConsulClientOption.Token;
                });
                //consul实例获取
                var entrys = client.Health.Service(serviceName).GetAwaiter().GetResult();
                services = entrys.Response.Select(i => i.Service).ToArray();

                if (services.Length > 0)
                {
                    _agentServices.AddOrUpdate(serviceName, services, (k, oldValue) => services);
                }
      
            }
      
        }

        /// <summary>
        /// 根据不同策略  获得不同的索引
        /// </summary>
        /// <returns></returns>
        protected abstract int GetIndex(int length);

        /// <summary>
        /// 维护服务列表的健康地址
        /// </summary>
        /// <returns></returns>
        public async Task CheckHealthService()
        {
            var serviceNames = _agentServices.Select(i => i.Key);

            foreach (var name in serviceNames)
            {
                using ConsulClient client = new(c =>
                {
                    c.Address = new Uri(_ConsulClientOption.ConsulAddress);
                    c.Token = _ConsulClientOption.Token;
                });
                //consul实例获取
                var entrys = await client.Health.Service(name, null, true, new QueryOptions() { WaitTime = TimeSpan.FromMinutes(5) });
                var services = entrys.Response.Select(i => i.Service).ToArray();
                if (services.Length > 0)
                {
                    //更新
                    _agentServices.AddOrUpdate(name, services, (k, oldValue) => services);
                }
            }

        }

    }
}
