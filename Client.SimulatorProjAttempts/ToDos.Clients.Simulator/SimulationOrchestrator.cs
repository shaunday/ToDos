using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace ToDos.Clients.Simulator
{
    public class SimulationOrchestrator
    {
        private readonly IUnityContainer _container;
        public SimulationOrchestrator(IUnityContainer container)
        {
            _container = container;
        }

        public async Task Run(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Script file path required.");
            var scriptFile = args[0];
            var parser = _container.Resolve<ScriptFileParser>();
            var parseResult = parser.Parse(scriptFile);
            int baseUserId = parseResult.UserId ?? 1;
            int numClients = parseResult.NumOfClients;
            bool signToEvents = parseResult.SignToEvents ?? false;
            var tasks = Enumerable.Range(0, numClients).Select(async i =>
            {
                var app = _container.Resolve<SimulatorApp>();
                // Use baseUserId + i for unique user IDs
                await app.Run(new[] { (baseUserId + i).ToString(), scriptFile });
            });
            await Task.WhenAll(tasks);
        }
    }
} 