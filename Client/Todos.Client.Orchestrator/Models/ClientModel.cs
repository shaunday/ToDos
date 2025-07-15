using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Todos.Client.Common;
using Todos.Client.Common.Factories;

namespace Todos.Client.Orchestrator.Services
{
    public partial class ClientModel : ObservableObject
    {
        public TypesGlobal.ClientType ClientType { get; }
        public Process Process { get; }
        public string LogFilePath { get; }
        public string LogFileName { get; }
        public int ProcessId => Process.Id;

        [ObservableProperty]
        private bool isAlive;

        public ClientModel(TypesGlobal.ClientType clientType, Process process)
        {
            ClientType = clientType;
            Process = process;
            var logFileName = LogFactory.GetLogFileName(process.Id, clientType);
            LogFileName = logFileName;
            LogFilePath = LogFactory.GetLogFilePath(logFileName);
            IsAlive = !process.HasExited;

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                IsAlive = false;
            };
        }
    }
} 