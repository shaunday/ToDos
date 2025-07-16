using System;
using System.Threading.Tasks;

namespace ToDos.HeadlessSimulators
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] Entry {DateTime.Now}\n");
            Console.WriteLine("ToDos Headless Simulator started.");
            if (args.Length == 0)
            {
                System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] No args {DateTime.Now}\n");
                Console.WriteLine("Usage: ToDos.HeadlessSimulators <userid> <scriptfile> | --orchestrate <scriptfile>");
                return;
            }

            try
            {
                System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] Try block entered {DateTime.Now}\n");
                var argParser = new ArgumentParser();
                var scriptParser = new ScriptFileParser();
                var appFactory = new Func<SimulatorApp>(() => new SimulatorApp(argParser, scriptParser, null));
                System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] Factory created {DateTime.Now}\n");

                if (args[0] == "--orchestrate")
                {
                    System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] Orchestrate mode {DateTime.Now}\n");
                    var orchestrator = new SimulationOrchestrator(appFactory);
                    // Replace args[1..] with manual array copy for .NET Framework compatibility
                    var orchestrateArgs = new string[args.Length - 1];
                    Array.Copy(args, 1, orchestrateArgs, 0, args.Length - 1);
                    Task.Run(async () =>
                    {
                        await orchestrator.Run(orchestrateArgs);
                    }).GetAwaiter().GetResult();
                }
                else
                {
                    System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] App mode {DateTime.Now}\n");
                    var app = appFactory();
                    Task.Run(async () =>
                    {
                        await app.Run(args);
                    }).GetAwaiter().GetResult();
                }
                System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] Simulation complete {DateTime.Now}\n");
                Console.WriteLine("Simulation complete.");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("crash_diag.txt", $"[Program.Main] Exception: {ex}\n");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
