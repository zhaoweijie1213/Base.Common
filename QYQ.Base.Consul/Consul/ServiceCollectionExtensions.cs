using Consul;
using Consul.AspNetCore;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QYQ.Base.Consul.Consul;
using QYQ.Base.Consul.DispatcherExtend;
using QYQ.Base.Consul.Grpc;
using QYQ.Base.Consul.Grpc.Resolve;
using QYQ.Base.Consul.Grpc.Serivce;
using QYQ.Base.Consul.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace QYQ.Base.Consul
{
    /// <summary>
    /// consul扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        #region 注册ConsulClient

        /// <summary>
        /// 注册ConsulClient
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="consul">consul配置文件的key</param>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQConsul(this WebApplicationBuilder builder,string consul = "ConsulOptions")
        {

            builder.Services.AddQYQConsul(builder.Configuration, consul);
            return builder;
        }

        /// <summary>
        /// 注册ConsulClient
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="consul">consul配置文件的key</param>
        /// <returns></returns>
        public static IServiceCollection AddQYQConsul(this IServiceCollection services,IConfiguration configuration, string consul = "ConsulOptions")
        {
            var serviceOptions = configuration.GetSection(consul).Get<ConsulServiceOptions>();
            services.Configure<ConsulServiceOptions>(configuration.GetSection(consul));


            //添加consul客户端配置
            services.AddConsul(client =>
            {
                client.Address = new Uri(serviceOptions.ConsulAddress);
                client.Token = serviceOptions.Token;
            });
            return services;
        }

        #endregion


        #region 注册Agent

        #region http 注册
        /// <summary>
        /// 注册Consul http Agent
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="consul">consul配置文件的key</param>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQConsulHttp(this WebApplicationBuilder builder, string consul = "ConsulOptions")
        {
            builder.Services.AddQYQConsulHttp(builder.Configuration, consul);
            return builder;
        }

        /// <summary>
        /// 注册Consul http Agent
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="consul">consul配置文件的key</param>
        /// <returns></returns>
        public static IServiceCollection AddQYQConsulHttp(this IServiceCollection services, IConfiguration configuration, string consul = "ConsulOptions")
        {
            var serviceOptions = configuration.GetSection(consul).Get<ConsulServiceOptions>();
            //代理配置
            var agent = serviceOptions.ConsulAgents.FirstOrDefault(i => i.AgentCategory == AgentCategory.HTTP);
            //添加环境
            agent.Meta.Add("Env", configuration["apollo:Env"]);
            if (agent.Port != 0)
            {
                // 服务ID必须保证唯一
                agent.ServiceId = Guid.NewGuid().ToString();
                string ip = GetIPAddress();
                services.AddQYQConsulAgentService(options =>  //注册服务
                {
                    options.ID = agent.ServiceId;
                    options.Name = agent.ServiceName;
                    options.Tags = agent.Tags;
                    options.Meta = agent.Meta;
                    options.Address = ip;
                    options.Port = agent.Port;
                    options.Check = new AgentServiceCheck()
                    {
                        // 注册超时
                        Timeout = TimeSpan.FromSeconds(5),
                        // 服务停止多久后注销服务
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                        // 健康检查地址
                        HTTP = $"http://{ip}:{agent.Port}/api/Health",
                        // 健康检查时间间隔
                        Interval = TimeSpan.FromSeconds(10),
                    };

                });
            }
            services.AddHealthChecks();
            return services;
        }

        #endregion


        #region GRPC注册
        /// <summary>
        /// gRPC注册consul
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="consul">consul配置文件的key</param>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQConsulgRPC(this WebApplicationBuilder builder, string consul = "ConsulOptions")
        {
            builder.Services.AddQYQConsulgRPC(builder.Configuration, consul);
            return builder;
        }

        /// <summary>
        /// gRPC注册consul
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="consul">consul配置文件的key</param>
        /// <returns></returns>
        public static IServiceCollection AddQYQConsulgRPC(this IServiceCollection services,IConfiguration configuration, string consul = "ConsulOptions")
        {
            var serviceOptions = configuration.GetSection(consul).Get<ConsulServiceOptions>();

            var agent = serviceOptions.ConsulAgents.FirstOrDefault(i => i.AgentCategory == AgentCategory.GRPC);
            //添加环境
            agent.Meta.Add("Env", configuration["apollo:Env"]);
            if (agent.Port != 0)
            {
                // 服务ID必须保证唯一
                agent.ServiceId = Guid.NewGuid().ToString();
                string ip = GetIPAddress();
                services.AddQYQConsulAgentService(options =>  //注册服务
                {
                    options.ID = agent.ServiceId;
                    options.Name = agent.ServiceName;
                    options.Tags = agent.Tags;
                    options.Meta = agent.Meta;
                    options.Address = ip;
                    options.Port = agent.Port;
                    options.Check = new AgentServiceCheck()
                    {
                        //gRPC特有
                        GRPC = $"{ip}:{agent.Port}",
                        //支持http
                        GRPCUseTLS = false,
                        // 注册超时
                        Timeout = TimeSpan.FromSeconds(5),
                        // 服务停止多久后注销服务
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                        // 健康检查时间间隔
                        Interval = TimeSpan.FromSeconds(10),
                    };

                });
            }
            return services;
        }


        #endregion


        /// <summary>
        /// 注册consul agent
        /// Register consul service with default <see cref="IConsulClient"/>.
        /// First client can be accessed from DI, to create multiple named clients use <see cref="IConsulClientFactory"/>
        /// </summary>
        public static IServiceCollection AddQYQConsulAgentService(
            this IServiceCollection services,
            Action<AgentServiceRegistration> configure)
        {
            var registration = new AgentServiceRegistration();

            configure.Invoke(registration);

            services.AddSingleton(registration);

            return services;
        }


        /// <summary>
        /// 添加注册任务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQRegistrationHostedService(this WebApplicationBuilder builder)
        {
             builder.Services.AddQYQRegistrationHostedService();

            return builder;
        }

        /// <summary>
        /// 添加注册任务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQYQRegistrationHostedService(this IServiceCollection services)
        {
            return services.AddHostedService<AgentServiceRegistrationQYQHostedService>();
        }

        #endregion

        /// <summary>
        /// 注册Consul调度策略
        /// 默认为轮询
        /// </summary>
        /// <param name="services"></param>
        /// <param name="consulDispatcherType"></param>
        public static void AddConsulDispatcher(
            this IServiceCollection services,
            ConsulDispatcherType consulDispatcherType = ConsulDispatcherType.Polling)
        {
            switch (consulDispatcherType)
            {
                case ConsulDispatcherType.Average:
                    services.AddSingleton<AbstractConsulDispatcher, AverageDispatcher>();
                    break;
                case ConsulDispatcherType.Polling:
                    services.AddSingleton<AbstractConsulDispatcher, PollingDispatcher>();
                    break;
                case ConsulDispatcherType.Weight:
                    services.AddSingleton<AbstractConsulDispatcher, WeightDispatcher>();
                    break;
                default:
                    break;
            }

            //维护服务健康地址的列表
            services.AddHostedService<AgentServiceHealthCheck>();
        }

        /// <summary>
        /// 添加consul 发送HTTP请求处理
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        public static IServiceCollection AddConsulHttpClient(this IServiceCollection services, string name)
        {
            services.AddTransient<ConsulHttpMessageHandler>();
            services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri($"http://{name}");
                client.DefaultRequestHeaders.Add("ConsulServiceName", name);
            }).AddHttpMessageHandler<ConsulHttpMessageHandler>();

            return services;

        }

        /// <summary>
        /// 添加consul Grpc客户端
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <param name="consulServiceName"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddConsulGrpcClient<TClient>(this WebApplicationBuilder builder, string name, string consulServiceName)
            where TClient : class
        {
            builder.Services.AddConsulGrpcClient<TClient>(name, consulServiceName, builder.Configuration);
            return builder;
        }

        /// <summary>
        /// 添加consul 发送Grpc请求处理
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name">客户端实例名</param>
        /// <param name="consulServiceName">consul注册的服务名</param>
        /// <param name="configuration">配置信息</param>
        public static IServiceCollection AddConsulGrpcClient<TClient>(this IServiceCollection services, string name,string consulServiceName,IConfiguration configuration)
            where TClient : class
        {

            //services.Configure<GrpcConsulServiceNameOptions>(name, options =>
            //{
            //    options.Metadata = new Metadata()
            //    {
            //        { "Consul-Service", consulServiceName }
            //    };
            //});
   
            services.AddGrpcClient<TClient>(name, client =>
            {
                Uri addresss = configuration.GetSection("ConsulOptions:ConsulAddress").Get<Uri>();
                client.Address = new Uri($"consul://{addresss.Host}:{addresss.Port}");
                client.ChannelOptionsActions.Add(channel =>
                {
                    //channel.HttpHandler = new SocketsHttpHandler
                    //{
                    //    //当达到并发流限制时，通道会创建额外的 HTTP/2 连接
                    //    EnableMultipleHttp2Connections = true,
                    //    //PooledConnectionIdleTimeout = TimeSpan.FromHours(30),
                    //    KeepAlivePingDelay = TimeSpan.FromSeconds(10),
                    //    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    //    // ...configure other handler settings
                    //};
                    channel.Credentials = ChannelCredentials.Insecure;
                    //channel.MaxRetryAttempts = 1000;
                    //channel.MaxSendMessageSize= int.MaxValue;
                    //channel.MaxReceiveMessageSize = null;
                    var serviceConfig = new ServiceConfig();
                    serviceConfig.Inner.Add("ServiceName", consulServiceName);
                    //配置通道
                    channel.ServiceConfig = serviceConfig;
                });
            });

            //consul负载均衡器
            services.AddSingleton<ResolverFactory, GrpcConsulResolverFactory>();
            //services.AddTransient<ConsulGrpcClientFactory>();
            return services;
        }

        /// <summary>
        /// 添加健康检查地址
        /// </summary>
        /// <param name="app"></param>
        public static void UseHealthcheck(this WebApplication app)
        {
            app.MapHealthChecks("api/Health");
        }       
        
        
        /// <summary>
        /// 添加健康检查地址
        /// </summary>
        /// <param name="app"></param>
        public static void UseHealthcheck(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("api/Health");
            });
        }

        /// <summary>
        /// 添加Grpc健康检查地址
        /// </summary>
        /// <param name="app"></param>
        public static void UseGrpcHealthcheck(this WebApplication app)
        {
            app.MapGrpcService<HealthCheckService>();
        }       
        
        
        /// <summary>
        /// 添加Grpc健康检查地址
        /// </summary>
        /// <param name="app"></param>
        public static void UseGrpcHealthcheck(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<HealthCheckService>();
            });
        }

        /// <summary>
        /// 获取ip地址
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            string ip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                        .Select(p => p.GetIPProperties())
                        .SelectMany(p => p.UnicastAddresses)
                        .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
                        .FirstOrDefault()?.Address.ToString();

            return ip;
        }

    }




    /// <summary>
    /// 调度类型
    /// </summary>
    public enum ConsulDispatcherType
    {
        /// <summary>
        /// 平均
        /// </summary>
        Average = 0,

        /// <summary>
        /// 轮询
        /// </summary>
        Polling = 1,

        /// <summary>
        /// 权重
        /// </summary>
        Weight = 2
    }
}
