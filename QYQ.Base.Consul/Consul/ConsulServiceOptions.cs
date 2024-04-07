using System;
using System.Collections.Generic;

namespace QYQ.Base.Consul
{
    /// <summary>
    ///  Consul配置模型类
    /// </summary>
    public class ConsulServiceOptions
    {
        /// <summary>
        /// 服务注册地址（Consul的地址）
        /// </summary>
        public string ConsulAddress { get; set; }

        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 代理配置
        /// </summary>
        public List<ConsulAgentOptions> ConsulAgents { get; set; }
      
    }

    /// <summary>
    /// 代理配置项
    /// </summary>
    public class ConsulAgentOptions
    {
        /// <summary>
        /// 代理类型
        /// </summary>
        public AgentCategory AgentCategory { get; set; }

        /// <summary>
        ///  服务ID
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        ///  服务名称
        /// </summary>
        public string ServiceName { get; set; }

        ///// <summary>
        ///// 站点地址
        ///// </summary>
        //public string ServiceAddress { get; set; }

        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; set; }


        /// <summary>
        /// 标签
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// 代理类型
    /// </summary>
    public enum AgentCategory
    {
        /// <summary>
        /// 
        /// </summary>
        HTTP = 0,

        /// <summary>
        /// 
        /// </summary>
        GRPC = 1,
    }
}
