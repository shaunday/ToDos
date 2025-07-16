using System;

namespace ToDos.Clients.Simulator
{
    public class SimulatorApp
    {
        private readonly ArgumentParser _argParser;
        private readonly ScriptFileParser _scriptParser;
        private readonly OperationExecutor _executor;

        public SimulatorApp(ArgumentParser argParser, ScriptFileParser scriptParser, OperationExecutor executor)
        {
            _argParser = argParser;
            _scriptParser = scriptParser;
            _executor = executor;
        }

        public void Run(string[] args)
        {
            var simArgs = _argParser.Parse(args);
            var scriptLines = _scriptParser.Parse(simArgs.FilePath);
            _executor.Execute(simArgs.UserId, scriptLines);
        }
    }

    public class SimulatorArgs
    {
        public int UserId { get; set; }
        public string FilePath { get; set; }
    }
} 