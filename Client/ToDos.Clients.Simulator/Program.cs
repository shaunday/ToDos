using System;
using Unity;
using System.Linq;

namespace ToDos.Clients.Simulator
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
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
