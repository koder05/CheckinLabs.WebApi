using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckinLabs.AppBase
{
    public class StartModulesLauncher : IHostedService
    {
        private IServiceProvider serviceProvider;
        public StartModulesLauncher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var startModules = scope.ServiceProvider.GetServices<StartModuleBase>();
                if (startModules != null)
                {
                    foreach (var m in startModules)
                    {
                        if (!CheckModuleDependencyConsistency(m))
                            throw new Exception($"Module {m.GetType().Name}: dependency consistency failure");
                    }

                    await Task.WhenAll(startModules.Select(m => m.RunAsync(cancellationToken, BuildModuleDependencyTaskWaitList(m, startModules))));
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var startModules = scope.ServiceProvider.GetServices<StartModuleBase>();
                if (startModules != null)
                {
                    await Task.Run(() =>
                    {
                        foreach (var d in startModules.Where(m => m.GetType().IsAssignableFrom(typeof(IDisposable))).Cast<IDisposable>())
                            d.Dispose();
                    });
                }
            }
        }
        private IEnumerable<Task> BuildModuleDependencyTaskWaitList(StartModuleBase module, IEnumerable<StartModuleBase> regiteredModules)
        {
            var depList = module.GetType().GetCustomAttributes(typeof(ModuleDependencyAttribute), false)
                .Cast<ModuleDependencyAttribute>()
                .Select(attr => attr.DependencyType)
                .Where(t => t != null);
            return regiteredModules.Where(m => depList.Contains(m.GetType())).Select(m => m.CompletionSource.Task);
        }
        private bool CheckModuleDependencyConsistency(StartModuleBase module)
        {
            var tree = new Dictionary<Type, Type>();
            BuildModuleDependencyTree(module.GetType(), tree);
            return !tree.ContainsKey(module.GetType());
        }
        private void BuildModuleDependencyTree(Type moduleType, Dictionary<Type, Type> tree)
        {
            var depList = moduleType.GetCustomAttributes(typeof(ModuleDependencyAttribute), false)
                .Cast<ModuleDependencyAttribute>()
                .Select(attr => attr.DependencyType)
                .Where(t => t != null);
            foreach (var type in depList)
            {
                if (!tree.ContainsKey(type))
                {
                    tree.Add(type, null);
                    BuildModuleDependencyTree(type, tree);
                }
            }
        }
    }
}
