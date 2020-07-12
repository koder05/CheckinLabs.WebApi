using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CheckinLabs.AppBase
{
    public static class CApp
    {
        public static IHostBuilder AddLifetimeModule<TModule>(this IHostBuilder builder) where TModule : class, IHostedService
        {
            builder.ConfigureServices((hostContext, services) => services.AddHostedService<TModule>());
            return builder;
        }
        public static IHostBuilder AddStartupModule<TModule>(this IHostBuilder builder) where TModule : StartModuleBase
        {
            builder.ConfigureServices((hostContext, services) =>
            {
                if (!services.Any(sd => sd.ServiceType == typeof(StartModulesLauncher)))
                {
                    services.AddHostedService<StartModulesLauncher>();
                    services.Add(new ServiceDescriptor(typeof(StartModulesLauncher), (svcProv) => svcProv.GetService<StartModulesLauncher>(), ServiceLifetime.Singleton));
                }
                services.AddSingleton<TModule>();
                services.Add(new ServiceDescriptor(typeof(StartModuleBase), (svcProv) => svcProv.GetService<TModule>(), ServiceLifetime.Singleton));
            });
            return builder;
        }
        public static IHostBuilder AddService<TService, TImplementation>(this IHostBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient) where TService : class where TImplementation : class, TService
        {
            builder.ConfigureServices((hostContext, services) =>
            {
                services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
            });
            return builder;
        }
        public static IHostBuilder AddService<TService>(this IHostBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient) where TService : class
        {
            builder.ConfigureServices((hostContext, services) =>
            {
                services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), lifetime));
            });
            return builder;
        }
        public static IHostBuilder AddService<TService>(this IHostBuilder builder, Func<IServiceProvider, object> factory, ServiceLifetime lifetime = ServiceLifetime.Transient) where TService : class
        {
            builder.ConfigureServices((hostContext, services) =>
            {
                services.Add(new ServiceDescriptor(typeof(TService), factory, lifetime));
            });
            return builder;
        }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
