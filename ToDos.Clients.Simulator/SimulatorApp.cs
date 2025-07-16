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
            string filePath = null;
            int? userId = null;
            bool? signToEvents = null;
            if (args.Length == 0)
            {
                filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "ExampleScript.txt");
            }
            else
            {
                var simArgs = _argParser.Parse(args);
                userId = simArgs.UserId;
                filePath = simArgs.FilePath;
            }
            var parseResult = _scriptParser.Parse(filePath);
            if (parseResult.UserId.HasValue)
                userId = parseResult.UserId;
            if (parseResult.SignToEvents.HasValue)
                signToEvents = parseResult.SignToEvents;
            if (!userId.HasValue)
                throw new ArgumentException("UserId must be specified either as a command-line argument or in the script file header.");
            _executor.Logger.Info("Simulator starting with userId={UserId}, signToEvents={SignToEvents}, script={Script}", userId, signToEvents, filePath);
            _executor.Execute(userId.Value, parseResult.ScriptLines, signToEvents ?? false);
        }
    }

    public class SimulatorArgs
    {
        public int UserId { get; set; }
        public string FilePath { get; set; }
    }
} 