using Consul;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QYQ.Base.Consul.DispatcherExtend
{
    /// <summary>
    /// 权重
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="consulClientOption"></param>
    public class WeightDispatcher(IOptionsMonitor<ConsulServiceOptions> consulClientOption) : AbstractConsulDispatcher(consulClientOption)
    {
        //public readonly ConcurrentDictionary<string, int[]> _serviceIndex = new();

        ///// <summary>
        ///// 按权重选择地址
        ///// </summary>
        ///// <param name="serviceName"></param>
        //public override async Task<string> ChooseAddress(string serviceName)
        //{
        //    await InitAgentServiceDictionary(serviceName);
        //    AgentService agentService = null;
        //    if (!_agentServices.TryGetValue(serviceName, out AgentService[] services))
        //    {
        //        foreach (var service in services)
        //        {
        //            if (int.TryParse(service.Meta?["weight"], out int iWeight))
        //            {
        //                for (int i = 0; i < iWeight; i++)
        //                {
        //                    serviceDictionaryNew.Add(service);
        //                }
        //            }
        //        }
        //        //更新服务地址
        //        _agentServices.AddOrUpdate(serviceName, serviceDictionaryNew.ToArray(), (k, oldValue) => serviceDictionaryNew.ToArray());
        //    }

        //    int index = new Random().Next(0, _agentServices[serviceName].Length);
        //    agentService = _agentServices[serviceName][index];
        //    return agentService != null ? $"{agentService.Address}:{agentService.Port}" : "";
        //}



        /// <summary>
        /// 按权重获取索引
        /// </summary>
        /// <returns></returns>
        protected override int GetIndex(string serviceName)
        {
            // 获取当前服务的所有实例
            AgentService[] services = _agentServices[serviceName];

            // 用于存储权重分布的索引集合
            List<int> indexCollection = new();

            foreach (var service in services)
            {
                // 当前元素的索引
                int itemIndex = Array.IndexOf(services, service);

                // 尝试获取服务的权重，如果不存在或无法解析为整数，则默认权重为1
                int iWeight = 1;
                if (service.Meta != null && service.Meta.TryGetValue("weight", out string weightValue))
                {
                    int.TryParse(weightValue, out iWeight); // 如果解析失败，则 iWeight 保持为默认值 1
                }

                // 按权重添加索引到集合中
                for (int i = 0; i < iWeight; i++)
                {
                    indexCollection.Add(itemIndex);
                }
            }

            if (indexCollection.Count == 0)
            {
                throw new InvalidOperationException("No services available for selection.");
            }

            // 随机选择一个索引
            int index = new Random().Next(0, indexCollection.Count);
            return indexCollection[index];
        }

    }
}
