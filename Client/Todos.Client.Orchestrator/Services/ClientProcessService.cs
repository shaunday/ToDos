using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Todos.Client.Common;
using Serilog;

namespace Todos.Client.Orchestrator.Services
{
    public class ClientProcessService
    {
        private readonly ILogger _logger;
        public ClientProcessService(ILogger logger)
        {
            _logger = logger;
        }
        public ObservableCollection<ClientModel> Clients { get; } = new ObservableCollection<ClientModel>();

        public void AddClient(TypesGlobal.ClientType clientType, Process process)
        {
            var model = new ClientModel(clientType, process);
            _logger.Information($"[Orch] AddClient: {model.LogFilePath}");
            Clients.Add(model);
        }

        public void RemoveClient(ClientModel model)
        {
            Clients.Remove(model);
        }

        public void RemoveByProcess(Process process)
        {
            var model = Clients.FirstOrDefault(c => c.Process == process);
            if (model != null)
                Clients.Remove(model);
        }

        public void KillClient(ClientModel model)
        {
            try
            {
                if (!model.Process.HasExited)
                    model.Process.Kill();
            }
            catch { /* log if needed */ }
            // Do not remove from Clients; just update IsAlive
            model.IsAlive = false;
        }

        public void KillAllClients()
        {
            foreach (var model in Clients.ToList())
            {
                KillClient(model);
            }
        }

        public ClientModel[] Filter(
            TypesGlobal.ClientType? clientType = null,
            bool? isAlive = null,
            int? processId = null)
        {
            return Clients.Where(c =>
                (!clientType.HasValue || c.ClientType == clientType.Value) &&
                (!isAlive.HasValue || c.IsAlive == isAlive.Value) &&
                (!processId.HasValue || c.ProcessId == processId.Value)
            ).ToArray();
        }
    }
} 