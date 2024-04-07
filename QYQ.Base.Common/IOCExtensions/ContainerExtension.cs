using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QYQ.Base.Common.IOCExtensions
{
    /// <summary>
    /// 扩展IServiceCollection批量注入方法
    /// </summary>
    public static class ContainerExtension
    {
        /// <summary>
        /// 要扫描的程序集名称,默认为[^Shop.Utils|^Shop.]多个使用|分隔
        /// </summary>
        private static string MatchAssemblies { get; set; } = $"^{AppDomain.CurrentDomain.FriendlyName}";

        /// <summary>
        /// 注入多个服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="matchAssemblies">要扫描的程序集名称正则表达式,默认为[^XXXX.XXXX|^XXXX.]多个使用|分隔</param>
        /// <returns></returns>
        public static IServiceCollection AddMultipleService(this IServiceCollection services, string matchAssemblies = "")
        {
            #region 依赖注入
            MatchAssemblies = matchAssemblies;
            //services.AddScoped<IUserService, UserService>();           
     
            var path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var getFiles = Directory.GetFiles(path, "*.dll").Where(Match);  //.Where(o=>o.Match())
            var referencedAssemblies = getFiles.Select(Assembly.LoadFrom).ToList();  //.Select(o=> Assembly.LoadFrom(o))         

            var ss = referencedAssemblies.SelectMany(o => o.GetTypes());

            //查找所有继承IDependency的类型
            var baseType = typeof(IDependency);
            var types = referencedAssemblies
                .SelectMany(a => a.DefinedTypes)
                .Select(type => type.AsType())
                .Where(x => x != baseType && baseType.IsAssignableFrom(x)).ToList();
            //获取所有类
            var implementTypes = types.Where(x => x.IsClass).ToList();
            //获取所有接口
            var interfaceTypes = types.Where(x => x.IsInterface).ToList();
            foreach (var implementType in implementTypes)
            {
                //判断类需要注入的生命周期
                if (typeof(IScopeDependency).IsAssignableFrom(implementType))
                {
                    //查找接口
                    var interfaceType = interfaceTypes.FirstOrDefault(x => x.IsAssignableFrom(implementType));
                    if (interfaceType != null)
                    {
                        services.AddScoped(interfaceType, implementType);
                    }
                    else
                    {
                        services.AddScoped(implementType);
                    }
                }
                else if (typeof(ISingletonDependency).IsAssignableFrom(implementType))
                {
                    //查找接口
                    var interfaceType = interfaceTypes.FirstOrDefault(x => x.IsAssignableFrom(implementType));
                    if (interfaceType != null)
                    {
                        services.AddSingleton(interfaceType, implementType);
                    }
                    else
                    {
                        services.AddSingleton(implementType);
                    }
                }
                else
                {
                    //查找接口
                    var interfaceType = interfaceTypes.FirstOrDefault(x => x.IsAssignableFrom(implementType));
                    if (interfaceType != null)
                    {
                        services.AddTransient(interfaceType, implementType);
                    }
                    else
                    {
                        services.AddTransient(implementType);
                    }
                }
            }
            #endregion
            return services;
        }

        /// <summary>
        /// 程序集是否匹配
        /// </summary>
        public static bool Match(string assemblyName)
        {
            if (string.IsNullOrEmpty(MatchAssemblies))
            {
                return true;
            }

            assemblyName = Path.GetFileName(assemblyName);
            if (assemblyName.StartsWith($"{AppDomain.CurrentDomain.FriendlyName}.Views"))
                return false;
            if (assemblyName.StartsWith($"{AppDomain.CurrentDomain.FriendlyName}.PrecompiledViews"))
                return false;
            return Regex.IsMatch(assemblyName, MatchAssemblies, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }
}
