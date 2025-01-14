using NSwag;
using QYQ.Base.Common.Middleware;
using QYQ.Base.Consul;
using QYQ.Base.Swagger.Extension;
using QYQ.Base.SnowId;
using EasyCaching.Redis;
using QYQ.Base.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLog4Net();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddConsulDispatcher(ConsulDispatcherType.Weight);
builder.AddQYQConsul().AddQYQConsulHttp().AddQYQConsulgRPC();

builder.Services.AddConsulHttpClient("game-play");
builder.AddConsulGrpcClient<GamePlay.Grpc.GamePlay.GamePlayClient>("Gameplay", "game-play-grpc");
builder.AddQYQSwaggerAndApiVersioning(new OpenApiInfo { Title = "CommonTest" }, new Asp.Versioning.ApiVersion(1));

//builder.Services.AddSnowIdRedisGenerator(builder.Configuration.GetSection("Redis").Get<RedisDBOptions>());
builder.Services.AddDefaultSnowIdGenerator();

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
