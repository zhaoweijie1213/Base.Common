using EasyCaching.Redis;
using EasyCaching.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NSwag;
using QYQ.Base.Common;
using QYQ.Base.Common.Middleware;
using QYQ.Base.Consul;
using QYQ.Base.SnowId;
using QYQ.Base.Swagger.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLog4Net();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddConsulDispatcher(ConsulDispatcherType.Weight);
builder.AddQYQConsul().AddQYQConsulHttp().AddQYQConsulgRPC();

builder.Services.AddConsulHttpClient("game-play");
//builder.AddConsulGrpcClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay", "game-play-grpc");
builder.AddQYQSwaggerAndApiVersioning(new OpenApiInfo { Title = "CommonTest" }, new Asp.Versioning.ApiVersion(1));

#region EasyCaching×¢²á

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

app.Run();
