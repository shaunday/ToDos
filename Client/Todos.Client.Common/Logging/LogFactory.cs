using System;
using System.IO;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Client.Common.Factories
{
    public static class LogFactory
    {
        /// <summary>
        /// Generates a log file name based on process ID and client type.
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <param name="clientType">Client type enum</param>
        /// <returns>Log file name in the format "{clientType}_{pid}.log"</returns>
        public static string GetLogFileName(int pid, ClientType clientType)
        {
            if (pid <= 0)
                throw new ArgumentException("PID must be a positive integer", nameof(pid));

            string clientTypeName = clientType.ToString();
            return $"{clientTypeName}_{pid}.log";
        }

        public static string GetLogFilePath(string fileName)
        {
            // Use the directory of the running executable
            var baseDir = AppContext.BaseDirectory;
            var logsDir = Path.Combine(baseDir, "Logs");
            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);
            return Path.Combine(logsDir, fileName);
        }
    }
} 