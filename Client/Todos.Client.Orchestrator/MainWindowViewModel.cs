using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Todos.Client.Common;
using Todos.Client.Orchestrator.Services;
using Todos.Client.Orchestrator.ViewModels;

namespace Todos.Client.Orchestrator.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region Fields
        private readonly ClientProcessService _clientService = new ClientProcessService();
        #endregion

        #region Properties
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

        // --- Sim Launcher fields ---
        [ObservableProperty]
        private int simLaunchCount = 1;
        [ObservableProperty]
        private string simUserId = string.Empty;
        [ObservableProperty]
        private string simFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
        public string SimLauncherInstructions => "Count defaults to 1. User ID is optional. Folder Path is optional and defaults to the output directory. The launcher will attempt to load the folder path as relative to the output directory first, then as an absolute path if not found.";

        // --- Sim Launcher command ---
        [RelayCommand]
        private void LaunchClientSim()
        {
            int count = Math.Max(1, SimLaunchCount);
            string userId = string.IsNullOrWhiteSpace(SimUserId) ? "defaultUser" : SimUserId;
            string outputDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            string folder = string.IsNullOrWhiteSpace(SimFolderPath) ? outputDir : SimFolderPath;
            // Try relative first, then absolute
            string relPath = System.IO.Path.Combine(outputDir, SimFolderPath);
            if (!string.IsNullOrWhiteSpace(SimFolderPath) && System.IO.Directory.Exists(relPath))
                folder = relPath;
            else if (!string.IsNullOrWhiteSpace(SimFolderPath) && System.IO.Directory.Exists(SimFolderPath))
                folder = SimFolderPath;
            else if (!string.IsNullOrWhiteSpace(SimFolderPath))
                folder = relPath; // fallback to relative if not found
            var simExe = System.IO.Path.Combine(outputDir, "ToDos.Clients.Simulator.exe");
            if (!System.IO.File.Exists(simExe))
            {
                System.Windows.MessageBox.Show($"Simulator executable not found in: {outputDir}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var args = $"--user {userId} --folder {folder}";
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = simExe,
                    WorkingDirectory = outputDir,
                    Arguments = args
                };
                System.Diagnostics.Process.Start(startInfo);
            }
        }

        [RelayCommand]
        private void OpenSimExampleFile()
        {
            string outputDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            string exampleFile = System.IO.Path.Combine(outputDir, "ExampleScript.txt");
            if (System.IO.File.Exists(exampleFile))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exampleFile) { UseShellExecute = true });
            }
            else
            {
                System.Windows.MessageBox.Show($"Example file not found: {exampleFile}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [ObservableProperty]
        private bool launchByFileConfig;
        [ObservableProperty]
        private string fileConfigPath = @"users.txt";
        public string FileConfigDescription => "File format: one user per line: userId displayName password count\nExample: user1 John 1234 3";

        public ICollectionView FilteredClientsView { get; }
        public int TotalClientCount => _clientService.Clients.Count;
        public int FilteredClientCount => FilteredClientsView?.Cast<object>().Count() ?? 0;
        public LogViewerViewModel LogViewerViewModel { get; }
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            FilteredClientsView = CollectionViewSource.GetDefaultView(_clientService.Clients);
            FilteredClientsView.Filter = FilterClientPredicate;
            // Set default filter to alive only
            FilterIsAlive = true;
            // Subscribe to process Exited for all existing clients
            foreach (var client in _clientService.Clients)
            {
                if (client.Process != null)
                {
                    client.Process.EnableRaisingEvents = true;
                    client.Process.Exited += (s, e) => OnClientProcessExited(client);
                }
            }
            // Initial filter
            FilteredClientsView.Refresh();
            // Listen for changes to update counts
            FilteredClientsView.CollectionChanged += (s, e) => NotifyClientCountsChanged();
            FilteredClientsView.CurrentChanged += (s, e) => NotifyClientCountsChanged();
            _clientService.Clients.CollectionChanged += (s, e) => NotifyClientCountsChanged();
            LogViewerViewModel = new LogViewerViewModel(new LogFileWatcherService());
            FilteredClientsView.CollectionChanged += (s, e) => UpdateLogViewerLogFiles();
            FilteredClientsView.CurrentChanged += (s, e) => UpdateLogViewerLogFiles();
            UpdateLogViewerLogFiles();
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ClearFilters()
        {
            FilterClientType = null;
            FilterIsAlive = null;
            FilterProcessIdText = string.Empty;
        }

        private bool FilterClientPredicate(object obj)
        {
            if (!(obj is ClientModel c)) return false;
            if (FilterClientType.HasValue && c.ClientType != FilterClientType.Value) return false;
            if (FilterIsAlive.HasValue && c.IsAlive != FilterIsAlive.Value) return false;
            if (!string.IsNullOrWhiteSpace(FilterProcessIdText) && !c.ProcessId.ToString().Contains(FilterProcessIdText)) return false;
            return true;
        }

        [RelayCommand]
        private void LaunchClient()
        {
            int count = Math.Max(1, LaunchClientCount);
            var outputDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            var clientExe = System.IO.Path.Combine(outputDir, "TodDos.Ui.exe");
            if (!System.IO.File.Exists(clientExe))
            {
                System.Windows.MessageBox.Show($"Client executable not found in: {outputDir}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = clientExe,
                    WorkingDirectory = outputDir
                };
                var proc = System.Diagnostics.Process.Start(startInfo);
                if (proc != null)
                {
                    _clientService.AddClient(TypesGlobal.ClientType.UiClient, proc);
                    proc.EnableRaisingEvents = true;
                    proc.Exited += (s, e) => OnClientProcessExited(_clientService.Clients.FirstOrDefault(c => c.Process == proc));
                }
            }
            FilteredClientsView.Refresh();
        }

        [RelayCommand]
        private void KillClient(ClientModel model)
        {
            if (model == null) return;
            _clientService.KillClient(model);
            FilteredClientsView.Refresh();
        }

        [RelayCommand]
        private void KillAllClients()
        {
            if (_clientService.Clients.Count == 0) return;
            var result = System.Windows.MessageBox.Show($"Are you sure you want to kill all {_clientService.Clients.Count} running client(s)?", "Confirm Kill All", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _clientService.KillAllClients();
                FilteredClientsView.Refresh();
            }
        }

        [RelayCommand]
        private void OpenLog(ClientModel model)
        {
            if (model == null) return;
            System.Console.WriteLine($"[DEBUG] Attempting to open log file: {model.LogFilePath}");
            if (System.IO.File.Exists(model.LogFilePath))
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(model.LogFilePath) { UseShellExecute = true });
            else
                System.Windows.MessageBox.Show($"Log file not found: {model.LogFilePath}", "Log Not Found", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
            FilteredClientsView.Refresh();
        }

        [RelayCommand]
        private void Simulate()
        {
            // SimulateAddTaskAsync(selectedSimulatorPid, ...);
        }

        [RelayCommand]
        private void LaunchClientsByFileConfig()
        {
            if (string.IsNullOrWhiteSpace(FileConfigPath) || !System.IO.File.Exists(FileConfigPath))
            {
                System.Windows.MessageBox.Show($"File not found: {FileConfigPath}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            var outputDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            var clientExe = System.IO.Path.Combine(outputDir, "TodDos.Ui.exe");
            if (!System.IO.File.Exists(clientExe))
            {
                System.Windows.MessageBox.Show($"Client executable not found in: {outputDir}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            var lines = System.IO.File.ReadAllLines(FileConfigPath);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;
                string user = parts[0];
                string displayName = parts[1]; // not used, but could be
                string pass = parts[2];
                if (!int.TryParse(parts[3], out int count)) count = 1;
                for (int i = 0; i < count; i++)
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = clientExe,
                        WorkingDirectory = outputDir,
                        Arguments = $"user={user};pass={pass}"
                    };
                    var proc = System.Diagnostics.Process.Start(startInfo);
                    if (proc != null)
                    {
                        _clientService.AddClient(TypesGlobal.ClientType.UiClient, proc);
                        proc.EnableRaisingEvents = true;
                        proc.Exited += (s, e) => OnClientProcessExited(_clientService.Clients.FirstOrDefault(c => c.Process == proc));
                    }
                }
            }
            FilteredClientsView.Refresh();
        }
        #endregion

        #region Methods
        partial void OnFilterClientTypeChanged(TypesGlobal.ClientType? value) => FilteredClientsView.Refresh();
        partial void OnFilterIsAliveChanged(bool? value) => FilteredClientsView.Refresh();
        partial void OnFilterProcessIdTextChanged(string value) => FilteredClientsView.Refresh();

        private void NotifyClientCountsChanged()
        {
            OnPropertyChanged(nameof(TotalClientCount));
            OnPropertyChanged(nameof(FilteredClientCount));
        }

        public void OnFilterChanged()
        {
            FilteredClientsView.Refresh();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private void OnClientProcessExited(ClientModel client)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // Remove client and refresh view
            App.Current.Dispatcher.Invoke(() =>
            {
                FilteredClientsView.Refresh();
                NotifyClientCountsChanged();
            });
        }

        private void UpdateLogViewerLogFiles()
        {
            var logFilePaths = FilteredClientsView.Cast<ClientModel>()
                .Select(c => c.LogFilePath)
                .Where(System.IO.File.Exists)
                .ToList();
            LogViewerViewModel.UpdateLogFiles(logFilePaths);
        }
        #endregion
    }
} 