using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Todos.Client.Common;
using Todos.Client.Common.Factories;

namespace Todos.Client.Orchestrator.Services
{
    public class ClientModel : ObservableObject
    {
        public TypesGlobal.ClientType ClientType { get; }
        public Process Process { get; }
        public string LogFilePath { get; }
        public string LogFileName { get; }
        public int ProcessId => Process.Id;

        private bool isAlive;
        public bool IsAlive
        {
            get => isAlive;
            private set => SetProperty(ref isAlive, value);
        }

        public ClientModel(TypesGlobal.ClientType clientType, Process process)
        {
            ClientType = clientType;
            Process = process;
            var logFileName = LogFactory.GetLogFileName(process.Id, clientType);
            LogFileName = logFileName;
            LogFilePath = LogFactory.GetLogFilePath(logFileName);
            isAlive = !process.HasExited;

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                IsAlive = false;
            };
        }
    }
} 