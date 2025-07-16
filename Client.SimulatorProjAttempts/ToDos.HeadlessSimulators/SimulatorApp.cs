using System;
using System.Threading.Tasks;

namespace ToDos.HeadlessSimulators
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

        private OperationExecutor CreateExecutor(int userId, bool signToEvents)
        {
            return new OperationExecutor(userId, signToEvents);
        }

        public async Task Run(string[] args)
        {
            System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] Entry {DateTime.Now}\n");
            string filePath = null;
            int? userId = null;
            bool signToEvents = false;
            if (args.Length == 0)
            {
                filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "ExampleScript.txt");
            }
            else
            {
                System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] Parsing args {DateTime.Now}\n");
                var simArgs = _argParser.Parse(args);
                userId = simArgs.UserId;
                filePath = simArgs.FilePath;
            }
            System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] Parsing script {filePath} {DateTime.Now}\n");
            var parseResult = _scriptParser.Parse(filePath);
            if (parseResult.UserId.HasValue)
                userId = parseResult.UserId;
            if (parseResult.SignToEvents.HasValue)
                signToEvents = parseResult.SignToEvents.Value;
            if (!userId.HasValue)
            {
                System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] No userId {DateTime.Now}\n");
                throw new ArgumentException("UserId must be specified either as a command-line argument or in the script file header.");
            }
            System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] Creating executor {DateTime.Now}\n");
            var executor = CreateExecutor(userId.Value, signToEvents);
            System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] Executing script {DateTime.Now}\n");
            await executor.Execute(userId.Value, parseResult.ScriptLines);
            System.IO.File.AppendAllText("crash_diag.txt", $"[SimulatorApp.Run] Exit {DateTime.Now}\n");
        }
    }
} 