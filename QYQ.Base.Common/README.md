# 公共组件

`QYQ.Base.Common` 是一个面向 ASP.NET Core 8 的公共组件库，封装常用的中间件、认证、配置与依赖注入扩展，帮助快速搭建统一的 Web API 基础设施。

## 功能概览
- **统一返回格式**：内置 `ApiResult<T>` 与 `ApiResultFilter`，保证接口响应字段一致。
- **全局异常处理**：`UseErrorHandling` 捕获未处理异常并输出标准化错误。
- **HTTP 请求日志**：`UseQYQHttpLogging` 结合 `QYQHttpLoggingOptions` 控制忽略路径、请求/响应体大小与慢查询阈值。
- **JWT 身份验证**：`AddQYQAuthentication` 读取 `jwt` 节点自动配置 `JwtBearer`。
- **Serilog 集成**：`AddQYQSerilog` 读取配置文件完成 Serilog 注册并附加机器名、上下文信息。
- **Apollo 配置**：`AddQYQApollo` 根据 `apollo` 节点装配配置中心，支持通过 `Cluster` 覆盖集群。
- **Apollo 原文 Namespace**：`AddQYQRawNamespace` 接入内容无法被内置解析器摊平的 Namespace（如策划表格导出的 XML），原样取回整段原文。
- **XML 配置映射**：`[XmlAttributeName]` + `GetXmlItems<T>` 把 XML 原文映射成业务对象；`AddQYQXmlOptions<T>` 再接成 Apollo 推送即刷新的配置选项。
- **批量依赖注入**：`AddMultipleService` 基于 `IDependency` 标记接口按生命周期批量注册，实现约定优于配置。

## 安装

在目标项目目录执行：

```bash
dotnet add package QYQ.Base.Common
```

或者在 `.csproj` 中添加引用：

```xml
<PackageReference Include="QYQ.Base.Common" Version="8.6.6" />
```

## 环境要求
- .NET 8.0 及以上。
- 若使用 Apollo，需要在配置文件中提供 `apollo` 节点并可选指定 `Cluster`。
- Serilog、JWT 等功能依赖对应配置节点，缺失时会抛出异常或使用默认值。

## 配置示例
在 `appsettings.json` 中准备与组件对应的配置：

```json
{
  "jwt": {
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "SecretKey": "your-secret-key-should-be-longer-than-16",
    "AccessExpiration": 60,
    "RefreshExpiration": 1440
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console"],
    "MinimumLevel": "Information",
    "WriteTo": [ { "Name": "Console" } ]
  },
  "QYQHttpLoggingOptions": {
    "IgnorePath": ["/api/Health", "/swagger"],
    "MaxRequestBodySize": 1048576,
    "MaxResponseLenght": 1048576,
    "LongQueryThreshold": 100
  },
  "apollo": {
    "AppId": "your-app-id",
    "Env": "DEV",
    "Meta": { "DEV": "http://your.apollo.meta" }
  }
}
```

## 快速上手

### 1. 注册服务与中间件

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddQYQApollo();
builder.Services.AddQYQAuthentication(builder.Configuration);
builder.Services.AddQYQSerilog(builder.Configuration);
builder.Services.AddQYQHttpLogging(options =>
{
    options.IgnorePath.Add("/health");
    options.LongQueryThreshold = 200;
});
builder.Services.AddMultipleService("Your.Assembly.Pattern");
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResultFilter>();
});

var app = builder.Build();

app.UseErrorHandling();
app.UseAuthentication();
app.UseAuthorization();
app.UseQYQHttpLogging();

app.MapControllers();
app.Run();
```

### 2. ApiResult 统一返回格式

`ApiResult<T>` 提供标准化的接口返回模型，包含 `Code`、`Message` 与 `Data` 三个字段。配合 `QYQBaseController` 可快速构建统一响应：

```csharp
[ApiController]
[Route("api/[controller]")]
public class DemoController : QYQBaseController
{
    [HttpGet("hello")]
    public ApiResult<string> Hello()
    {
        return Json(ApiResultCode.Success, "world");
    }
}
```

如果启用 `ApiResultFilter`，模型验证错误会自动转换为统一结构的响应，便于前端处理。

### 3. Apollo 配置扩展
`AddQYQApollo` 会读取 `apollo` 节点的 `Meta` 与 `Env` 信息构造配置中心地址，并允许通过 `Cluster` 覆盖集群名称。

### 4. JWT 鉴权扩展
`AddQYQAuthentication` 依赖 `jwt` 节点参数（发行方、受众、密钥与过期时间）自动装配 `JwtBearer`，并将参数注入 `IOptions<JwtSettings>` 供业务使用。

### 5. Serilog 与 HTTP 日志
`AddQYQSerilog` 直接读取 Serilog 配置，附加机器名与上下文信息；`UseQYQHttpLogging` 会记录请求方法、路径、耗时以及请求/响应体，且可通过 `QYQHttpLoggingOptions` 控制忽略路径与大小阈值。

### 6. 批量依赖注入
`AddMultipleService` 会扫描匹配到的程序集，查找实现 `IDependency` 族接口的类型，并根据 `ISingletonDependency`、`IScopeDependency` 或默认瞬时生命周期完成批量注册，减少手动配置。

### 7. Apollo 原文 Namespace 与 XML 配置映射

策划表格导出的 XML 常是同名兄弟节点 + 中文属性名，客户端内置的 `XmlConfigAdapter` 会撞键抛 `FormatException` 导致服务启动即崩。`AddQYQRawNamespace` 把整段原文原样放在 `{sectionKey}:content` 上，解析权交回业务侧：

```csharp
builder.Configuration
    .AddQYQApollo()
    .AddQYQRawNamespace("base-game-list", "BaseGameList");
```

用 `[XmlAttributeName]` 声明 DTO 属性对应 XML 上的哪个属性名（允许中文），传泛型 `T` 即可拿到集合：

```csharp
public class GameListEntry
{
    [XmlAttributeName("ID")]       public int Id { get; set; }
    [XmlAttributeName("游戏ID")]   public string Title { get; set; } = string.Empty;
    [XmlAttributeName("GameID")]   public int GameId { get; set; }
    [XmlAttributeName("DistrictID", Required = false)] public int? DistrictId { get; set; }
}

// 直接读配置
IReadOnlyList<GameListEntry> list = configuration.GetXmlItems<GameListEntry>("BaseGameList");

// 或解析任意一段原文
IReadOnlyList<GameListEntry> list2 = xmlContent.ParseXmlItems<GameListEntry>("item");
```

需要 Apollo 推送即刷新时，用 `AddQYQXmlOptions<T>` 注册成配置选项：

```csharp
builder.Services.AddQYQXmlOptions<GameListEntry>("BaseGameList");

public class GameListService(IOptionsMonitor<XmlConfigOptions<GameListEntry>> options)
{
    public IReadOnlyList<GameListEntry> Items => options.CurrentValue.Items;
}
```

几条需要注意的约定：

- 读取端**必须**用 `IOptionsMonitor<T>` 或 `IOptionsSnapshot<T>`；`IOptions<T>` 是单例快照，拿不到新值。
- `AddQYQRawNamespace` 的第一个参数**不含格式后缀**，后缀由 format 拼接：传 `"base-game-list"` 请求的是 `base-game-list.xml`。
- 适配器按格式注册在 Apollo 客户端的**全局静态表**上，调用后该格式（默认 `Xml`）的所有 Namespace 都不再自动摊平；同一进程内还有需要自动摊平的同格式 Namespace 时请勿使用。
- 这类 Namespace **不要**写进 `apollo:Namespaces` 数组——那条路径的 sectionKey 为 null，原文会落到配置根级并与其他非 properties Namespace 互相覆盖。
- 只映射 XML 属性（attribute），不读子元素文本。未标注 `[XmlAttributeName]` 的属性不参与映射。
- `Required` 默认为 `true`，节点上缺该属性即抛；`Required` 只管「缺不缺」，属性写了但值转换不了无论如何都抛。
- 原文为空（配置尚未下发）返回空集合、不抛；其余失败统一抛 `XmlContentParseException`，带行号、属性名与原始值。
- `AddQYQXmlOptions` 不做容错，原文畸形时 `CurrentValue` 会持续抛。需要「解析失败沿用旧快照」的场景请在业务侧自建单例并订阅 `IConfiguration.GetReloadToken`。

---
如需更多示例或详细实现，请查看源码。若有问题，欢迎通过仓库 Issue 反馈。
