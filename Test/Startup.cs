using EasyCaching.Redis;
using Grpc.Team;
using LobbyWebManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QYQ.Base.Common.IOCExtensions;
using QYQ.Base.Consul;
using QYQ.Base.SnowId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Startup
    {
 

        public void ConfigureHost(IHostBuilder hostBuilder) =>
           hostBuilder.ConfigureAppConfiguration(builder =>
           {
               builder.AddJsonFile("appsettings.json");
           })
           .ConfigureLogging(builder =>
           {
               builder.AddLog4Net();
           })
           .ConfigureServices((context, services) =>
           {
               services.AddControllers();
               services.AddQYQConsul(context.Configuration);
               services.AddConsulGrpcClient<Team.TeamClient>("team", "pokervegas-invite-job-grpc", context.Configuration).AddConsulDispatcher();
               services.AddConsulGrpcClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay", "game-play-grpc", context.Configuration);
               services.AddConsulHttpClient("game-integration").AddConsulDispatcher();
               services.AddSnowIdGenerator().AddWorderIdRegister(context.Configuration.GetSection("Redis").Get<RedisDBOptions>()!);
               services.AddScoped<GameIntegrationApi>();
           });


        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
        }
    }
}
