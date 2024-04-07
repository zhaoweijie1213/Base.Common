//using QYQ.Base.Consul.DispatcherExtend;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace QYQ.Base.Consul.Grpc
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class ConsulGrpcMessageHandler : DelegatingHandler
//    {

//        private readonly AbstractConsulDispatcher _abstractConsulDispatcher;

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="abstractConsulDispatcher"></param>
//        public ConsulGrpcMessageHandler(AbstractConsulDispatcher abstractConsulDispatcher)
//        {
//            _abstractConsulDispatcher = abstractConsulDispatcher;
//        }

//        /// <summary>
//        /// 发送HTTP请求
//        /// </summary>
//        /// <param name="request"></param>
//        /// <param name="cancellationToken"></param>
//        /// <returns></returns>
//        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
//        {

//            //string servieName = request.RequestUri.Host;
//            //获取consul注册的名字
//            //string servieName = request.Content.Headers.GetValues("ConsulServiceName").First();
//            string servieName = request.RequestUri.Host;

//            string requestPath = _abstractConsulDispatcher.ChooseAddress(servieName);
//            string pathQuery = request.RequestUri.PathAndQuery;

//            request.RequestUri = new Uri(requestPath + pathQuery);

//            return base.Send(request, cancellationToken);
//        }

//        /// <summary>
//        /// 发送HTTP请求
//        /// </summary>
//        /// <param name="request"></param>
//        /// <param name="cancellationToken"></param>
//        /// <returns></returns>
//        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {

//            string servieName = request.RequestUri.Host;
//            string requestPath = _abstractConsulDispatcher.ChooseAddress(servieName);
//            string pathQuery = request.RequestUri.PathAndQuery;

//            request.RequestUri = new Uri(requestPath + pathQuery);

//            return await base.SendAsync(request, cancellationToken);
//        }
//    }
//}
