using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Todos.Client.Common;
using Todos.Client.Orchestrator.Services;

namespace Todos.Client.Orchestrator.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ClientProcessService _clientService = new ClientProcessService();

        [ObservableProperty]
        private TypesGlobal.ClientType? filterClientType;
        [ObservableProperty]
        private bool? filterIsAlive;
        [ObservableProperty]
        private string filterProcessIdText = string.Empty;
        [ObservableProperty]
        private ClientModel selectedClient;

        [ObservableProperty]
        private TypesGlobal.ClientType launchClientType = TypesGlobal.ClientType.UiClient;
        [ObservableProperty]
        private int launchClientCount = 1;
        [ObservableProperty]
        private ObservableCollection<int> simulatorPids = new ObservableCollection<int>();
        [ObservableProperty]
        private int selectedSimulatorPid;
        [ObservableProperty]
        private ObservableCollection<string> simulatorCommands = new ObservableCollection<string> { "AddTask", "UpdateTask", "DeleteTask" };
        [ObservableProperty]
        private string selectedSimulatorCommand;

        public ObservableCollection<ClientModel> Clients => _clientService.Clients;

        public ObservableCollection<ClientModel> FilteredClients { get; } = new ObservableCollection<ClientModel>();

        [ObservableProperty]
        private string combinedLogs = string.Empty;

        public MainWindowViewModel()
        {
            // Initial filter
            UpdateFilteredClients();
        }

        partial void OnFilterClientTypeChanged(TypesGlobal.ClientType? value) => UpdateFilteredClients();
        partial void OnFilterIsAliveChanged(bool? value) => UpdateFilteredClients();
        partial void OnFilterProcessIdTextChanged(string value) => UpdateFilteredClients();

        [RelayCommand]
        private void ClearFilters()
        {
            FilterClientType = null;
            FilterIsAlive = null;
            FilterProcessIdText = string.Empty;
        }


        private void UpdateFilteredClients()
        {
            var filtered = _clientService.Clients.Where(c =>
                (!FilterClientType.HasValue || c.ClientType == FilterClientType.Value) &&
                (!FilterIsAlive.HasValue || c.IsAlive == FilterIsAlive.Value) &&
                (string.IsNullOrWhiteSpace(FilterProcessIdText) || c.ProcessId.ToString().Contains(FilterProcessIdText))
            ).ToList();
            FilteredClients.Clear();
            foreach (var c in filtered)
            {
                Console.WriteLine($"[DEBUG] Filtered client: PID={c.ProcessId}, LogFilePath={c.LogFilePath}");
                FilteredClients.Add(c);
            }
        }

        [RelayCommand]
        private void LaunchClient()
        {
            // Launch as many clients as specified by LaunchClientCount (minimum 1)
            int count = Math.Max(1, LaunchClientCount);
            var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            var clientExe = Path.Combine(outputDir, "TodDos.Ui.exe");
            if (!File.Exists(clientExe))
            {
                MessageBox.Show($"Client executable not found in: {outputDir}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = clientExe,
                    WorkingDirectory = outputDir
                };
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    _clientService.AddClient(TypesGlobal.ClientType.UiClient, proc);
                    proc.EnableRaisingEvents = true;
                    proc.Exited += (s, e) => Application.Current?.Dispatcher.Invoke(UpdateFilteredClients);
                }
            }
            UpdateFilteredClients();
        }

        [RelayCommand]
        private void KillClient(ClientModel model)
        {
            if (model == null) return;
            _clientService.KillClient(model);
            UpdateFilteredClients();
        }

        [RelayCommand]
        private void KillAllClients()
        {
            if (Clients.Count == 0) return;
            var result = MessageBox.Show($"Are you sure you want to kill all {Clients.Count} running client(s)?", "Confirm Kill All", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _clientService.KillAllClients();
                UpdateFilteredClients();
            }
        }

        [RelayCommand]
        private void OpenLog(ClientModel model)
        {
            if (model == null) return;
            Console.WriteLine($"[DEBUG] Attempting to open log file: {model.LogFilePath}");
            if (File.Exists(model.LogFilePath))
                Process.Start(new ProcessStartInfo(model.LogFilePath) { UseShellExecute = true });
            else
                MessageBox.Show($"Log file not found: {model.LogFilePath}", "Log Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void DeleteSelected(System.Collections.IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0) return;
            var toDelete = selectedItems.Cast<ClientModel>().ToList();
            foreach (var model in toDelete)
            {
                _clientService.RemoveClient(model);
            }
            UpdateFilteredClients();
        }

        [RelayCommand]
        private void Simulate()
        {
            // SimulateAddTaskAsync(selectedSimulatorPid, ...);
        }

        public void OnFilterChanged()
        {
            UpdateFilteredClients();
        }
    }
} 