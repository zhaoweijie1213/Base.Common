using EasyCaching.Redis;
using EasyCaching.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NSwag;
using QYQ.Base.Common;
using QYQ.Base.Common.IOCExtensions;
using QYQ.Base.Common.Middleware;
using QYQ.Base.Consul;
using QYQ.Base.SnowId;
using QYQ.Base.Swagger.Extension;
using Test.Web.Models;
using static Grpc.ShortLink.ShortLink;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddQYQApollo().AddQYQRawNamespace("base-game-list", "BaseGameList");
builder.Logging.AddLog4Net();
// Add services to the container.

// 注册游戏列表配置，内部已接好配置变更令牌，Apollo 推送后自动重算
builder.Services.AddQYQXmlOptions<GameListEntry>("BaseGameList");


builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddConsulDispatcher(ConsulDispatcherType.Weight);
builder.AddQYQConsul().AddQYQConsulHttp().AddQYQConsulgRPC();
builder.AddConsulGrpcClient<ShortLinkClient>("short-link", "short-link-grpc");
builder.Services.AddConsulHttpClient("game-play");
//builder.AddConsulGrpcClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay", "game-play-grpc");
builder.AddQYQSwaggerAndApiVersioning(new OpenApiInfo { Title = "CommonTest" }, new Asp.Versioning.ApiVersion(1));

#region EasyCaching注册

builder.Services.AddEasyCaching(options =>
{
    var redis = builder.Configuration.GetSection("Redis").Get<RedisDBOptions>();
    Action<EasyCachingJsonSerializerOptions> easycaching = x =>
    {
        x.NullValueHandling = NullValueHandling.Ignore;
        x.TypeNameHandling = TypeNameHandling.None;
    };
    options.UseRedis(config =>
    {
        config.DBConfig = redis;
    }, "DefaultRedis").WithJson(easycaching, "DefaultRedis");
});

#endregion

builder.Services.AddSnowIdRedisGenerator(null, "DefaultRedis");
//builder.Services.AddDefaultSnowIdGenerator();

builder.Services.AddQYQHttpLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();
app.UseRouting();

app.UseQYQHttpLogging();

app.MapControllers();
app.UseGrpcHealthcheck();
app.UseHttpHealthcheck();
app.UseQYQSwaggerUI("CommonTest", true);

// 把 Apollo 客户端日志接进日志系统，长轮询与推送情况才看得见
app.UseQYQApolloLogger();

app.Run();
