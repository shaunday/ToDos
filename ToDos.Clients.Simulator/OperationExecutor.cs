using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;

namespace ToDos.Clients.Simulator
{
    public class OperationExecutor
    {
        private readonly ILogger _logger;
        public OperationExecutor(ILogger logger)
        {
            _logger = logger;
        }
        public void Execute(int userId, List<ScriptLine> scriptLines)
        {
            foreach (var line in scriptLines)
            {
                for (int i = 0; i < line.Count; i++)
                {
                    foreach (var op in line.Operations)
                    {
                        _logger.Information("[User {UserId}] Executing {Operation}", userId, op);
                        // TODO: Call TaskSyncClientAdapter for actual operation
                    }
                    if (line.DelayEach > 0)
                        Thread.Sleep(line.DelayEach);
                }
                if (line.DelayEnd > 0)
                    Thread.Sleep(line.DelayEnd);
            }
        }
    }
} 