# QYQ.Base.SnowId

雪花 ID 组件，支持默认随机 WorkerId 以及基于 Redis 的 WorkerId 注册模式。

**2025-08-23**

## 支持的目标框架
- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 9.0

## 安装
- 使用命令行安装：
  ```bash
  dotnet add package QYQ.Base.SnowId
  ```
- 通过 `PackageReference` 引用：
  ```xml
  <ItemGroup>
    <PackageReference Include="QYQ.Base.SnowId" Version="8.1.8" />
  </ItemGroup>
  ```

## 基于依赖注入的注册与使用
### 默认生成器（本地随机 WorkerId）
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QYQ.Base.SnowId;
using QYQ.Base.SnowId.Interface;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDefaultSnowIdGenerator(options =>
        {
            // 可选：自定义基础时间（UTC）
            options.BaseTime = new DateTime(2020, 2, 20, 2, 20, 2, 20, DateTimeKind.Utc);
        });
    });

using var host = builder.Build();
var generator = host.Services.GetRequiredService<ISnowIdGenerator>();
long id = generator.CreateId();
long customTimeId = generator.CreateId(DateTime.UtcNow.AddMinutes(-1));
```

### Redis 生成器（分布式 WorkerId 注册）
```csharp
using EasyCaching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QYQ.Base.SnowId;
using QYQ.Base.SnowId.Interface;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var redisOptions = context.Configuration.GetSection("SnowId:Redis").Get<RedisDBOptions>();
        services.AddSnowIdRedisGenerator(redisOptions);
    });

using var host = builder.Build();
var generator = host.Services.GetRequiredService<ISnowIdGenerator>();
long id = generator.CreateId();
long customTimeId = generator.CreateId(DateTime.UtcNow.AddSeconds(-30));
```
`AddSnowIdRedisGenerator` 会自动注册后台服务以完成 WorkerId 的申请、心跳刷新与注销。

## 配置与环境变量
### 默认生成器
- `SnowServerId`：可通过环境变量或配置提供当前实例的 WorkerId；未设置时将随机生成一个 0-19 的 WorkerId。
- `DataCenterId`：数据中心标识，默认值为 `0`，请保持在 0-3 之间。
- 可选 `SnowIdOptions:BaseTime`：基础时间（UTC），不配置则使用 `2020-02-20 02:20:02.020 (UTC)`。

典型 `appsettings.json` 片段：
```json
{
  "SnowServerId": 1,
  "DataCenterId": 0,
  "SnowIdOptions": {
    "BaseTime": "2020-02-20T02:20:02.020Z"
  }
}
```

### Redis 生成器
- `DataCenterId`：数据中心标识，默认值为 `0`。
- `SnowId:Redis`：`RedisDBOptions` 结构的完整 Redis 连接配置（密码、节点、库号等）。

典型 `appsettings.json` 片段：
```json
{
  "DataCenterId": 1,
  "SnowId": {
    "Redis": {
      "Password": "your_password",
      "AllowAdmin": true,
      "Endpoints": [
        { "Host": "127.0.0.1", "Port": 6379 }
      ],
      "Database": 0
    }
  }
}
```

## 生成 NuGet README
打包时 `QYQ.Base.SnowId/README.md` 会作为 NuGet README 文件随包发布，上述内容即为最新说明。
