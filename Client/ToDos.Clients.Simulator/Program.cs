using Serilog;
using System;
using System.Diagnostics;
using System.IO;
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


        public static string GetLogFileName(int pid, ClientType clientType)
        {
            if (pid <= 0)
                throw new ArgumentException("PID must be a positive integer", nameof(pid));

            string clientTypeName = clientType.ToString();
            return $"{clientTypeName}_{pid}.log";
        }
        public enum ClientType { UiClient, ClientSimulator }

        public static string GetLogFilePath(string fileName)
        {
            // Use the directory of the running executable
            var baseDir = AppContext.BaseDirectory;
            var logsDir = Path.Combine(baseDir, "Logs");
            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);
            return Path.Combine(logsDir, fileName);
        }
    }
}
