using System;
using Unity;

namespace ToDos.Clients.Simulator
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var container = new UnityContainer();
            UnityConfig.RegisterTypes(container);
            var app = container.Resolve<SimulatorApp>();
            await app.Run(args);
        }
    }
}
