using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Serilog;
using System;

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
            try
            {
                var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
                var clientExe = Path.Combine(outputDir, "TodDos.Ui.exe");
                
                Log.Information("Attempting to launch client from: {ClientExe}", clientExe);
                
                if (!File.Exists(clientExe))
                {
                    var errorMsg = $"Client executable not found in: {outputDir}";
                    Log.Error(errorMsg);
                    MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check for required dependencies
                var envFile = Path.Combine(outputDir, ".env.Global");
                if (!File.Exists(envFile))
                {
                    Log.Warning(".env.Global file not found in output directory: {EnvFile}", envFile);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = clientExe,
                    WorkingDirectory = outputDir,
                    UseShellExecute = false, // This allows us to capture output
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                };

                Log.Information("Starting client process with working directory: {WorkingDir}", outputDir);
                
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    Log.Information("Client process started successfully with PID: {Pid}", proc.Id);
                    
                    RunningClients.Add(proc);
                    proc.EnableRaisingEvents = true;
                    
                    // Handle process exit
                    proc.Exited += (s, e) =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Log.Information("Client process exited with PID: {Pid}, ExitCode: {ExitCode}", proc.Id, proc.ExitCode);
                            RunningClients.Remove(proc);
                        });
                    };

                    // Capture output for debugging
                    proc.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Log.Information("Client Output [{Pid}]: {Output}", proc.Id, e.Data);
                        }
                    };

                    proc.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Log.Error("Client Error [{Pid}]: {Error}", proc.Id, e.Data);
                        }
                    };

                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }
                else
                {
                    Log.Error("Failed to start client process");
                    MessageBox.Show("Failed to start client process", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception occurred while launching client");
                MessageBox.Show($"Error launching client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void KillClient(Process? proc)
        {
            if (proc == null) return;
            
            try
            {
                Log.Information("Attempting to kill client process with PID: {Pid}", proc.Id);
                
                if (!proc.HasExited)
                {
                    proc.Kill();
                    Log.Information("Successfully killed client process with PID: {Pid}", proc.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error killing client process with PID: {Pid}", proc.Id);
            }
            finally
            {
                RunningClients.Remove(proc);
            }
        }

        [RelayCommand]
        private void KillAllClients()
        {
            if (RunningClients.Count == 0) return;

            var result = MessageBox.Show(
                $"Are you sure you want to kill all {RunningClients.Count} running client(s)?",
                "Confirm Kill All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Log.Information("Killing all {Count} client processes", RunningClients.Count);
                
                var processesToKill = RunningClients.ToList(); // Create a copy to avoid collection modification issues
                
                foreach (var proc in processesToKill)
                {
                    try
                    {
                        if (!proc.HasExited)
                        {
                            proc.Kill();
                            Log.Information("Killed client process with PID: {Pid}", proc.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error killing client process with PID: {Pid}", proc.Id);
                    }
                }
                
                RunningClients.Clear();
                Log.Information("All client processes have been terminated");
            }
        }

        public void KillAllClientsSilent()
        {
            if (RunningClients.Count == 0) return;

            Log.Information("Silently killing all {Count} client processes", RunningClients.Count);
            
            var processesToKill = RunningClients.ToList();
            
            foreach (var proc in processesToKill)
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                        Log.Information("Killed client process with PID: {Pid}", proc.Id);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error killing client process with PID: {Pid}", proc.Id);
                }
            }
            
            RunningClients.Clear();
        }
    }
} 