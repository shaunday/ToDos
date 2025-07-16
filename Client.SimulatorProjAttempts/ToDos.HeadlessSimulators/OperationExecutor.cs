using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.HeadlessSimulators;
using ToDos.MockAuthService;
using Todos.Client.SignalRClient;
using ToDos.DotNet.Common;
using Serilog.Sinks.File;
using Serilog;

namespace ToDos.HeadlessSimulators
{
    public static class LogFactory
    {
        public static string GetLogFileName(int pid, string clientType)
        {
            if (pid <= 0)
                throw new ArgumentException("PID must be a positive integer", nameof(pid));
            return $"{clientType}_{pid}.log";
        }

        public static string GetLogFilePath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            var logsDir = System.IO.Path.Combine(baseDir, "Logs");
            if (!System.IO.Directory.Exists(logsDir))
                System.IO.Directory.CreateDirectory(logsDir);
            return System.IO.Path.Combine(logsDir, fileName);
        }
    }

    public class OperationExecutor
    {
        private readonly TaskSyncClientAdapter _adapter;
        public OperationExecutor(int userId, bool signToEvents)
        {
            System.IO.File.AppendAllText("crash_diag.txt", $"[OperationExecutor.Ctor] Entry {DateTime.Now}\n");
            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            string logFileName = LogFactory.GetLogFileName(pid, "ClientSimulator");
            string logFilePath = LogFactory.GetLogFilePath(logFileName);
            var logger = new Serilog.LoggerConfiguration().WriteTo.File(logFilePath).CreateLogger();
            var authService = new MockJwtAuthService(logger);
            _adapter = new TaskSyncClientAdapter(logger, authService);
            _adapter.Configure(userId, signToEvents);
            _adapter.ConnectAsync().GetAwaiter().GetResult();
            System.IO.File.AppendAllText("crash_diag.txt", $"[OperationExecutor.Ctor] Exit {DateTime.Now}\n");
        }

        public async Task Execute(int userId, List<ScriptLine> scriptLines)
        {
            System.IO.File.AppendAllText("crash_diag.txt", $"[OperationExecutor.Execute] Entry {DateTime.Now}\n");
            foreach (var line in scriptLines)
            {
                for (int i = 0; i < line.Count; i++)
                {
                    foreach (var op in line.Operations)
                    {
                        System.IO.File.AppendAllText("crash_diag.txt", $"[OperationExecutor.Execute] Operation: {op} {DateTime.Now}\n");
                        switch (op.Trim().ToLowerInvariant())
                        {
                            case "add":
                                await _adapter.AddTask(userId);
                                break;
                            case "delete":
                                await _adapter.DeleteTask(userId);
                                break;
                            case "lock":
                                await _adapter.LockTask(userId);
                                break;
                            case "unlock":
                                await _adapter.UnlockTask(userId);
                                break;
                            case "getall":
                                await _adapter.GetAll(userId);
                                break;
                            default:
                                System.IO.File.AppendAllText("crash_diag.txt", $"[OperationExecutor.Execute] Unknown op: {op} {DateTime.Now}\n");
                                Console.WriteLine($"Unknown operation: {op}");
                                break;
                        }
                    }
                    if (line.DelayEach > 0)
                        await Task.Delay(line.DelayEach);
                }
                if (line.DelayEnd > 0)
                    await Task.Delay(line.DelayEnd);
            }
            System.IO.File.AppendAllText("crash_diag.txt", $"[OperationExecutor.Execute] Exit {DateTime.Now}\n");
        }
    }
} 