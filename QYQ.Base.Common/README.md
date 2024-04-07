# 公共组件

> 扩展方法

## 错误处理中间件

```c#
        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            ...
        }
```

## Http日志中间件

```c#
      /// <summary>
      /// 添加http请求日志中间件
      /// </summary>
      /// <param name="app"></param>
      public static void UseQYQHttpLogging(this WebApplication app)
      {
          ...
      }
```

## Apollo扩展

```c#
        /// <summary>
        /// 添加apollo配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApolloConfigurationBuilder AddQYQApollo(this IConfigurationBuilder builder)
        {
            ...
        }
```

## 鉴权扩展

```c#
    /// <summary>
    /// 添加身份验证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddQYQAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        ...
    }
```

## IOC注册扩展

```c#
     /// <summary>
     /// 注入多个服务
     /// </summary>
     /// <param name="services"></param>
     /// <param name="matchAssemblies">要扫描的程序集名称正则表达式,默认为[^XXXX.XXXX|^XXXX.]多个使用|分隔</param>
     /// <returns></returns>
     public static IServiceCollection AddMultipleService(this IServiceCollection services, string matchAssemblies = "")
     {
     	...
     }
```

