using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using Todos.Client.Common;
using Todos.Client.Common.Factories;
using ToDos.MockAuthService;
using Unity;
using static Todos.Client.Common.TypesGlobal;

namespace ToDos.Clients.Simulator
{
    public static class UnityConfig
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterInstance<ILogger>(Log.Logger);

            container.RegisterType<ArgumentParser>();
            container.RegisterType<ScriptFileParser>();
            container.RegisterType<OperationExecutor>();
            container.RegisterType<SimulatorApp>();

            container.RegisterType<MockJwtAuthService>(new Unity.Lifetime.ContainerControlledLifetimeManager(),
              new Unity.Injection.InjectionConstructor(container.Resolve<Serilog.ILogger>()));

            container.RegisterType<TaskSyncClientAdapter>();
          
        }
    }
} 