# 公共组件

`QYQ.Base.Common` 是一个面向 ASP.NET Core 的公共组件库，封装常用的中间件与扩展方法，帮助快速搭建服务。

## 安装

在目标项目目录执行：

```bash
dotnet add package QYQ.Base.Common
```

或者在 `.csproj` 中添加引用：

```xml
<PackageReference Include="QYQ.Base.Common" Version="8.5.6" />
```

## 快速上手

### 1. 注册服务与中间件

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddQYQApollo();
builder.Services.AddQYQAuthentication(builder.Configuration);
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

`ApiResult<T>` 提供标准化的接口返回模型，包含 `Code`、`Message` 与 `Data` 三个字段。配合 `QYQBaseController` 可快速构建统一响应。

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

如需验证参数，可启用 `ApiResultFilter` 自动处理模型验证错误，使返回结构保持一致。

### 错误处理中间件

在管道起始处调用 `UseErrorHandling`，捕获未处理异常并返回统一格式的错误信息。

### Http 请求日志

`UseQYQHttpLogging` 会记录请求方法、路径、耗时等信息，便于分析问题和排查故障。

### Apollo 配置扩展

`AddQYQApollo` 将 Apollo 配置中心内容加载到 `IConfiguration`，支持动态刷新配置。

### 鉴权扩展

`AddQYQAuthentication` 根据配置快速启用 JWT 身份验证。

### IOC 注册扩展

`AddMultipleService` 按命名规则扫描并注入服务，实现依赖注入的自动化。

---

更多功能和示例可参考源码及文档。

