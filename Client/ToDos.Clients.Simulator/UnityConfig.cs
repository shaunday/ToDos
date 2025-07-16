using Unity;
using Serilog;
using Todos.Client.Common.Factories;
using Todos.Client.Common;
using static Todos.Client.Common.TypesGlobal;
using System.Diagnostics;
using ToDos.MockAuthService;

namespace ToDos.Clients.Simulator
{
    public static class UnityConfig
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<ArgumentParser>();
            container.RegisterType<ScriptFileParser>();
            container.RegisterType<OperationExecutor>();
            container.RegisterType<SimulatorApp>();
            container.RegisterType<TaskSyncClientAdapter>();
            container.RegisterType<MockJwtAuthService>(new Unity.Lifetime.ContainerControlledLifetimeManager(),
                new Unity.Injection.InjectionConstructor(container.Resolve<Serilog.ILogger>()));
            // Register TaskSyncClientAdapter and any dependencies here
            // Setup Serilog logger
            int pid = Process.GetCurrentProcess().Id;
            string logFileName = LogFactory.GetLogFileName(pid, ClientType.ClientSimulator);
            string logFilePath = LogFactory.GetLogFilePath(logFileName);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            container.RegisterInstance(Log.Logger);
        }
    }
} 