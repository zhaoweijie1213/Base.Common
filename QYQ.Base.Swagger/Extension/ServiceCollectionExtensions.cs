using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NJsonSchema.Generation;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Reflection.Metadata;
using System.Xml.Linq;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;

namespace QYQ.Base.Swagger.Extension
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {


        /// <summary>
        /// swagger文档和Api版本控制
        /// 需要在Api控制器声明特性:
        /// 例:      
        ///         [ApiVersion("1.0")]                                     Api版本号
        ///         [ApiExplorerSettings(GroupName = "v1")]                 分组,与在Swagger文档哪个版本相关
        ///         [Route("/api/v{version:apiVersion}/[controller]")]      路由路径
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="openApiInfo">swagger文档信息</param>
        /// <param name="defaultVersion">默认版本,不传默认版本为1.0</param>
        /// <param name="apiGateway"></param>
        /// <returns></returns>
        /// <returns></returns>
        public static WebApplicationBuilder AddQYQSwaggerAndApiVersioning(this WebApplicationBuilder builder, OpenApiInfo openApiInfo, ApiVersion? defaultVersion = null,bool apiGateway = true)
        {
            //.NET环境
            bool IsProduction = builder.Environment.IsProduction();
            builder.Services.AddQYQSwaggerAndApiVersioning(openApiInfo, IsProduction, apiGateway, defaultVersion);
            return builder;
        }


        /// <summary>
        /// swagger文档和Api版本控制
        /// 需要在Api控制器声明特性:
        /// 例:      
        ///         [ApiVersion("1.0")]                                     Api版本号
        ///         [ApiExplorerSettings(GroupName = "v1")]                 分组,与在Swagger文档哪个版本相关
        ///         [Route("/api/v{version:apiVersion}/[controller]")]      路由路径
        /// </summary>
        /// <param name="services"></param>
        /// <param name="openApiInfo">swagger文档信息</param>
        /// <param name="IsProduction"></param>
        /// <param name="apiGateway">apiGateway</param>
        /// <param name="defaultVersion">默认版本,不传默认版本为1.0</param>
        /// <returns></returns>
        public static IServiceCollection AddQYQSwaggerAndApiVersioning(this IServiceCollection services, OpenApiInfo openApiInfo, bool IsProduction, bool apiGateway, ApiVersion? defaultVersion = null)
        {
            //api版本控制
            services.AddApiVersioning(options =>
            {
                //这是可选的, 当设置为 true 时, API 将返回响应标头中支持的版本信息
                options.ReportApiVersions = true;
                //此选项将用于不提供版本的请求。默认情况下, 假定的 API 版本为1.0
                options.AssumeDefaultVersionWhenUnspecified = true;
                //版本号以什么形式，什么字段传递
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                // 此选项用于指定在请求中未指定版本时要使用的默认 API 版本。这将默认版本为1.0
                if (defaultVersion == null)
                {
                    options.DefaultApiVersion = new ApiVersion(1.0);
                }
                else
                {
                    options.DefaultApiVersion = defaultVersion;
                }
            }).AddApiExplorer(options =>
            {
                //以通知swagger替换控制器路由中的版本并配置api版本
                options.SubstituteApiVersionInUrl = true;
                // 版本名的格式：v+版本号
                options.GroupNameFormat = "'v'VVV";
                //options.DefaultApiVersion = new ApiVersion(1, 0);
                //是否提供API版本服务
                //options.AssumeDefaultVersionWhenUnspecified = true;
            });

            #region swagger
            //services.AddSwaggerGen();
            //services.AddOptions<SwaggerGenOptions>()
            //    .Configure<IApiVersionDescriptionProvider>((options, service) =>
            //    {
            //        // 文件下载类型
            //        options.MapType<FileContentResult>(() => new OpenApiSchema() { Type = "file" });
            //        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            //        // 添加文档信息
            //        foreach (var item in service.ApiVersionDescriptions)
            //        {
            //            options.SwaggerDoc(item.GroupName, CreateInfoForApiVersion(item, openApiInfo));
            //        }
            //        //options.EnableAnnotations(); // 启用注解
            //        // 加载所有xml注释，这里会导致swagger加载有点缓慢
            //        var xmls = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            //        foreach (var xml in xmls)
            //        {
            //            options.IncludeXmlComments(xml, true);
            //        }
            //        options.SchemaFilter<EnumSchemaFilter>();

            //        options.AddSecurityDefinition("Bearer",
            //            new OpenApiSecurityScheme()
            //            {
            //                Description = "直接在下框输入JWT生成的Token",
            //                Name = "Authorization",
            //                In = ParameterLocation.Header,
            //                Type = SecuritySchemeType.Http,
            //                Scheme = "Bearer",
            //                BearerFormat = "JWT"
            //            });

            //        var securityRequirement = new OpenApiSecurityRequirement
            //        {
            //            {
            //                new OpenApiSecurityScheme
            //                {
            //                    Reference = new OpenApiReference
            //                    {
            //                        Type = ReferenceType.SecurityScheme, Id = "Bearer"
            //                    }
            //                },
            //                new List<string>()
            //            }
            //        };
            //        options.AddSecurityRequirement(securityRequirement);
            //    });
            #endregion
            //services.AddOpenApiDocument(document =>
            //{

            //    //      new OperationSecurityScopeProcessor("Bearer"));
            //    if (IsProduction)
            //    {
            //        document.PostProcess = procss =>
            //        {
            //            procss.DocumentPath = $"/{openApiInfo.Title}/swagger/swagger.json";
            //            var paths = procss.Paths.ToList();
            //            foreach (var path in paths)
            //            {
            //                procss.Paths.Remove(path);
            //                procss.Paths.Add($"/{openApiInfo.Title}{path.Key}", path.Value);
            //            }
            //        };
            //    }
            //});


            var provider = services.BuildServiceProvider()
                                         .GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                services
                  .AddOpenApiDocument(document =>
                  {
                      document.Title = openApiInfo.Title;
                      document.Description = openApiInfo.Description;
                      document.AddSecurity("JwtBearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme()
                      {
                          Description = "这是方式一(直接在输入框中输入认证信息，不需要在开头添加Bearer)",
                          Name = "Authorization",//jwt默认的参数名称
                          In = OpenApiSecurityApiKeyLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                          Type = OpenApiSecuritySchemeType.Http,
                          Scheme = "bearer"
                      });
                      //document.OperationProcessors.Add(
                      //    new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
                      document.DocumentName = "v" + description.ApiVersion.ToString();
                      document.ApiGroupNames = new[] { description.GroupName };

                      //枚举处理
                      document.SchemaSettings.SchemaProcessors.Add(new EnumProcessor());
                      if (IsProduction && apiGateway)
                      {
                          document.PostProcess = procss =>
                          {
                              procss.DocumentPath = $"/{openApiInfo.Title}/{document.DocumentName}/swagger.json";
                              var paths = procss.Paths.ToList();
                              foreach (var path in paths)
                              {
                                  procss.Paths.Remove(path);
                                  procss.Paths.Add($"/{openApiInfo.Title}{path.Key}", path.Value);
                              }
                          };
                      }


                  });
            }


            return services;
        }

        /// <summary>
        /// swaggerUI扩展
        /// </summary>
        /// <param name="app"></param>
        /// <param name="name">name与请求路径名相关</param>
        /// <param name="apiGateway"></param>
        /// <returns></returns>
        public static WebApplication UseQYQSwaggerUI(this WebApplication app, string name, bool apiGateway = true)
        {
            if (app.Configuration.GetSection("apollo:Env").Get<string>() == "Dev" || app.Environment.IsDevelopment())
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new SystemException("请配置名称!");
                }
                app.UseOpenApi(options =>
                {
                    options.PostProcess = (document, request) =>
                    {
                        //清理掉NSwag加上去的
                        document.Servers.Clear();
                    };
                });
                app.UseSwaggerUi(settings =>
                {
                    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    // 为每个版本创建一个JSON
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        //往SwaggerUI页面head标签中添加我们自己的代码，比如引入一些样式文件，或者执行自己的一些脚本代码
                        //options.HeadContent += $"<script type='text/javascript'>alert('欢迎来到SwaggerUI页面')</script>";

                        //设置请求路径
                        if (app.Environment.IsProduction() && apiGateway)
                        {
                            settings.SwaggerRoutes.Add(new SwaggerUiRoute(description.ApiVersion.ToString(), $"/{name}/swagger/v{description.ApiVersion}/swagger.json"));
                        }
                    }
                });
            }

            return app;
        }

        /// <summary>
        /// 添加文档信息
        /// </summary>
        /// <param name="description"></param>
        /// <param name="openApiInfo"></param>
        /// <returns></returns>
        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description , OpenApiInfo openApiInfo)
        {
            //var info = new OpenApiInfo()
            //{
            //    //标题
            //    Title = $"{openApiInfo.Title}",
            //    //当前版本
            //    Version = description.ApiVersion.ToString(),
            //    //文档说明
            //    Description = openApiInfo.Description,
            //    //联系方式
            //    Contact = new OpenApiContact() { Name = "zhaoweijie", Email = "zhaoweijie@queyouquan.net", Url = null },
            //    //TermsOfService = new Uri(""),
            //    //许可证
            //    //License = new OpenApiLicense() { Name = "文档", Url = new Uri("") }
            //};

            openApiInfo.Version = description.ApiVersion.ToString();

            //当有弃用标记时的提示信息
            if (description.IsDeprecated)
            {
                openApiInfo.Description += " - 此版本已放弃兼容";
            }
            return openApiInfo;
        }

    }
}
