using System;
using Unity;

namespace ToDos.Clients.Simulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            UnityConfig.RegisterTypes(container);
            var app = container.Resolve<SimulatorApp>();
            app.Run(args);
        }
    }
}
