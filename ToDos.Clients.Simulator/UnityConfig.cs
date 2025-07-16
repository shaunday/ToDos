using Unity;

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
            // Register TaskSyncClientAdapter and any dependencies here
        }
    }
} 