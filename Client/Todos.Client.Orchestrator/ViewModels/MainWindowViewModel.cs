using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Todos.Client.Orchestrator.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<Process> RunningClients { get; } = new ObservableCollection<Process>();

        public int RunningCount => RunningClients.Count;

        public MainWindowViewModel()
        {
            RunningClients.CollectionChanged += (s, e) => OnPropertyChanged(nameof(RunningCount));
        }

        [RelayCommand]
        private void LaunchClient()
        {
            var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            var clientExe = Path.Combine(outputDir, "TodDos.Ui.exe");
            if (!File.Exists(clientExe))
            {
                MessageBox.Show($"Client executable not found in: {outputDir}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = clientExe,
                WorkingDirectory = outputDir
            };
            var proc = Process.Start(startInfo);
            if (proc != null)
            {
                RunningClients.Add(proc);
                proc.EnableRaisingEvents = true;
                proc.Exited += (s, e) =>
                {
                    App.Current.Dispatcher.Invoke(() => RunningClients.Remove(proc));
                };
            }
        }

        [RelayCommand]
        private void KillClient(Process? proc)
        {
            if (proc == null) return;
            try
            {
                if (!proc.HasExited)
                    proc.Kill();
            }
            catch { /* ignore */ }
            RunningClients.Remove(proc);
        }
    }
} 