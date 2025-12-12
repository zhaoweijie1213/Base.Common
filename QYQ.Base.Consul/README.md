## QYQ.Base.Consul

### 库简介
QYQ.Base.Consul 提供基于 Consul 的服务注册与发现扩展，封装了客户端初始化、HTTP 与 gRPC 代理注册以及常用的调度策略配置，方便在 ASP.NET Core 应用中快速启用服务治理能力。

### 支持的目标框架
- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 9.0
- .NET 10.0

### 安装
```bash
dotnet add package QYQ.Base.Consul --version 8.3.1
```

### 基础配置与 AddQYQConsul 示例
在 `appsettings.json` 中按默认节名 `ConsulOptions` 配置 Consul 连接与代理信息：

```json
{
  "ConsulOptions": {
    "ConsulAddress": "http://localhost:8500",
    "Token": "",
    "HostIPAddress": "192.168.1.10",
    "ConsulAgents": [
      {
        "AgentCategory": 0,
        "ServiceId": "sample-api",
        "ServiceName": "sample-api",
        "Port": 5000,
        "Tags": ["http"],
        "Meta": { "Env": "dev" }
      },
      {
        "AgentCategory": 1,
        "ServiceId": "sample-grpc",
        "ServiceName": "sample-grpc",
        "Port": 6000,
        "Tags": ["grpc"],
        "Meta": { "Env": "dev" }
      }
    ]
  }
}
```

在 `Program.cs` 中添加基础客户端初始化：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 读取 ConsulOptions 节并注册 ConsulClient
builder.AddQYQConsul();
```

### HTTP 服务注册示例
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddQYQConsul();

// 注册 HTTP 代理并托管健康检查注销逻辑
builder.AddQYQConsulHttp();

var app = builder.Build();
app.MapGet("/api/Health", () => Results.Ok("healthy"));
app.Run();
```

### gRPC 服务注册示例
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddQYQConsul();

// 注册 gRPC 代理并托管健康检查注销逻辑
builder.AddQYQConsulgRPC();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.MapGet("/api/Health", () => "healthy");
app.Run();
```

### 自定义 gRPC 客户端重试与负载均衡
`AddConsulGrpcClient` 现支持通过可选的 `ServiceConfig` 委托自定义重试策略或禁用重试，内部仍会自动注入 Consul 解析与轮询负载均衡配置：

```csharp
// 自定义重试策略
services.AddConsulGrpcClient<Team.TeamClient>("team", "team-service", configuration, serviceConfig =>
{
    serviceConfig.MethodConfigs.Add(new MethodConfig
    {
        Names = { MethodName.Default },
        RetryPolicy = new RetryPolicy
        {
            MaxAttempts = 3,
            InitialBackoff = TimeSpan.FromSeconds(0.5),
            MaxBackoff = TimeSpan.FromSeconds(2),
            BackoffMultiplier = 1.2,
            RetryableStatusCodes = { StatusCode.Unavailable }
        }
    });
});

// 禁用重试
services.AddConsulGrpcClient<GamePlayClient>("game", "game-service", configuration, serviceConfig =>
{
    serviceConfig.MethodConfigs.Clear();
});
```

### 配置注意事项
- `ConsulOptions` 是默认配置节名，可通过传入其他节名覆写。
- `ConsulAddress` 与可选的 `Token` 用于连接 Consul 服务器，`HostIPAddress` 可显式指定注册地址。
- `ConsulAgents` 列表区分 `AgentCategory`（HTTP=0，GRPC=1），`ServiceId` 应保证唯一，`ServiceName` 需与 Consul 中的期望名称一致。
- 确保 `Port` 与健康检查路径/协议匹配：HTTP 示例使用 `/api/Health`，gRPC 示例需暴露对应端口供 Consul GRPC 健康检查。
- 如需自定义调度策略，可调用 `AddConsulDispatcher` 配置轮询或平均等策略，以便客户端侧负载均衡。

### 版本历史摘要
- 8.3.1：当前版本，支持 .NET 6.0 至 .NET 10.0，默认携带 HTTP/gRPC 注册托管服务与调度配置扩展。
- 更多历史版本与更新记录可在 NuGet 页面查看。
