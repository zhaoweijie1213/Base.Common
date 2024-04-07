using QYQ.Base.Consul.DispatcherExtend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QYQ.Base.Consul.Http
{
    /// <summary>
    /// consul发送http请求中间件
    /// </summary>
    public class ConsulHttpMessageHandler : DelegatingHandler
    {

        private readonly AbstractConsulDispatcher _abstractConsulDispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abstractConsulDispatcher"></param>
        public ConsulHttpMessageHandler(AbstractConsulDispatcher abstractConsulDispatcher)
        {
            _abstractConsulDispatcher = abstractConsulDispatcher;
        }



        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            //获取consul注册的名字
            string servieName = request.Headers.GetValues("ConsulServiceName").First();
            string requestPath = _abstractConsulDispatcher.ChooseAddress(servieName);
            string pathQuery = request.RequestUri.PathAndQuery;
            request.RequestUri = new Uri("http://" + requestPath + pathQuery);
            return base.Send(request, cancellationToken);
        }

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            //获取consul注册的名字
            string servieName = request.Headers.GetValues("ConsulServiceName").First();
            string requestPath = _abstractConsulDispatcher.ChooseAddress(servieName);
            string pathQuery = request.RequestUri.PathAndQuery;
            request.RequestUri = new Uri("http://" + requestPath + pathQuery);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
