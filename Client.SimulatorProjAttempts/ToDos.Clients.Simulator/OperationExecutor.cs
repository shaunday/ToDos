using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;

namespace ToDos.Clients.Simulator
{
    public class OperationExecutor
    {
        private readonly ILogger _logger;
        private readonly TaskSyncClientAdapter _taskSyncClient;
        public OperationExecutor(ILogger logger, TaskSyncClientAdapter taskSyncClient)
        {
            _logger = logger;
            _taskSyncClient = taskSyncClient;
        }
        public ILogger Logger => _logger;
        public void Execute(int userId, List<ScriptLine> scriptLines, bool signToEvents = false)
        {
            _taskSyncClient.Configure(userId, signToEvents);
            try
            {
                _taskSyncClient.ConnectAsync().GetAwaiter().GetResult();
                foreach (var line in scriptLines)
                {
                    for (int i = 0; i < line.Count; i++)
                    {
                        foreach (var op in line.Operations)
                        {
                            _logger.Information("[User {UserId}] Executing {Operation}", userId, op);
                            switch (op.Trim().ToLowerInvariant())
                            {
                                case "add":
                                    _taskSyncClient.AddTask(userId).GetAwaiter().GetResult();
                                    break;
                                case "delete":
                                    _taskSyncClient.DeleteTask(userId).GetAwaiter().GetResult();
                                    break;
                                case "lock":
                                    _taskSyncClient.LockTask(userId).GetAwaiter().GetResult();
                                    break;
                                case "unlock":
                                    _taskSyncClient.UnlockTask(userId).GetAwaiter().GetResult();
                                    break;
                                case "getall":
                                    _taskSyncClient.GetAll(userId).GetAwaiter().GetResult();
                                    break;
                                default:
                                    _logger.Information("Unknown operation: {Operation}", op);
                                    break;
                            }
                        }
                        if (line.DelayEach > 0)
                            Thread.Sleep(line.DelayEach);
                    }
                    if (line.DelayEnd > 0)
                        Thread.Sleep(line.DelayEnd);
                }
            }
            finally
            {
                _taskSyncClient.CleanupOnExit().GetAwaiter().GetResult();
            }
        }
    }
} 