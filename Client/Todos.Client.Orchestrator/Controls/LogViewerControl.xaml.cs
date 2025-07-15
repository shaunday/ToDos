using System.Collections.Generic;
using System.Windows.Controls;
using Todos.Client.Orchestrator.Services;
using Todos.Client.Orchestrator.ViewModels;

namespace Todos.Client.Orchestrator.Controls
{
    public partial class LogViewerControl : UserControl
    {
        private static LogFileWatcherService _logFileWatcherService = new LogFileWatcherService();
        public LogViewerViewModel ViewModel { get; }
        public LogViewerControl()
        {
            InitializeComponent();
            ViewModel = new LogViewerViewModel(_logFileWatcherService);
            DataContext = ViewModel;
        }
        public void UpdateLogFiles(List<string> logFilePaths)
        {
            ViewModel.UpdateLogFiles(logFilePaths);
        }
    }
} 