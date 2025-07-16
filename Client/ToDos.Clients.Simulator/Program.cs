using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using Todos.Client.Common.Factories;
using Unity;
using static Todos.Client.Common.TypesGlobal;

namespace ToDos.Clients.Simulator
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Setup Serilog logger
            int pid = Process.GetCurrentProcess().Id;
            string logFileName = LogFactory.GetLogFileName(pid, ClientType.ClientSimulator);
            string logFilePath = LogFactory.GetLogFilePath(logFileName);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var container = new UnityContainer();
            UnityConfig.RegisterTypes(container);
            if (args.Length > 0 && args[0] == "--orchestrate")
            {
                var orchestrator = new SimulationOrchestrator(container);
                await orchestrator.Run(args.Skip(1).ToArray());
            }
            else
            {
                var app = container.Resolve<SimulatorApp>();
                await app.Run(args);
            }
        }
    }
}
