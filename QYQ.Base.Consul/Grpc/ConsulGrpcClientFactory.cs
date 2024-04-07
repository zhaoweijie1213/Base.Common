//using Grpc.Net.ClientFactory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using QYQ.Base.Consul.DispatcherExtend;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace QYQ.Base.Consul.Grpc
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class ConsulGrpcClientFactory
//    {

//        private readonly GrpcClientFactory _grpcClientFactory;

//        private readonly IOptionsMonitor<GrpcClientFactoryOptions> _grpcClientFactoryOptionsMonitor;

//        //private readonly IConfigureOptions<GrpcClientFactoryOptions> _configureOptions;

//        private readonly AbstractConsulDispatcher _abstractConsulDispatcher;

//        private readonly IOptionsMonitor<GrpcConsulServiceNameOptions> _grpcConsulServiceNameOptions;


//        /// <summary>
//        /// 
//        /// </summary>
//        public ConsulGrpcClientFactory(GrpcClientFactory grpcClientFactory, IOptionsMonitor<GrpcClientFactoryOptions> grpcClientFactoryOptionsMonitor, AbstractConsulDispatcher abstractConsulDispatcher,
//            IOptionsMonitor<GrpcConsulServiceNameOptions> grpcConsulServiceNameOptions)
//        {
//            _grpcClientFactory = grpcClientFactory;
//            _grpcClientFactoryOptionsMonitor = grpcClientFactoryOptionsMonitor;
//            //_configureOptions = configureOptions;
//            _abstractConsulDispatcher = abstractConsulDispatcher;
//            _grpcConsulServiceNameOptions = grpcConsulServiceNameOptions;
//        }

//        /// <summary>
//        /// 创建grpc客户端
//        /// </summary>
//        /// <typeparam name="TClient"></typeparam>
//        /// <param name="name"></param>
//        /// <returns></returns>
//        /// <exception cref="NotImplementedException"></exception>
//        public TClient CreateClient<TClient>(string name)
//            where TClient : class
//        {
//            var config = _grpcClientFactoryOptionsMonitor.Get(name);

//            var consulNamesOptions = _grpcConsulServiceNameOptions.Get(name);
//            string consulServiceName = consulNamesOptions.Metadata.GetValue("Consul-Service");
//            string url = _abstractConsulDispatcher.ChooseAddress(consulServiceName);

//            if (string.IsNullOrEmpty(url))
//            {
//                throw new Exception("consul service address could not be found");
//            }
//            config.Address = new Uri($"http://{url}");
//            //_configureOptions.Configure(config);
//            return _grpcClientFactory.CreateClient<TClient>(name);
//        }
//    }
//}
