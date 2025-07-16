using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ToDos.HeadlessSimulators
{
    public class SimulationOrchestrator
    {
        private readonly Func<SimulatorApp> _appFactory;
        public SimulationOrchestrator(Func<SimulatorApp> appFactory)
        {
            _appFactory = appFactory;
        }

        public async Task Run(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Script file path required.");
            var scriptFile = args[0];
            // For simplicity, just run one SimulatorApp instance for now
            var app = _appFactory();
            await app.Run(new[] { "1", scriptFile });
        }
    }
} 