using System.Collections.Generic;
using System.Windows.Controls;
using Todos.Client.Orchestrator.Services;
using Todos.Client.Orchestrator.ViewModels;
using Serilog;
using System;

namespace Todos.Client.Orchestrator.Controls
{
    public partial class LogViewerControl : UserControl
    {
        static LogViewerControl()
        {
            Console.WriteLine("LogViewerControl static constructor called");
        }
        private static LogFileWatcherService _logFileWatcherService = new LogFileWatcherService();
        public LogViewerViewModel ViewModel { get; }
        public LogViewerControl(ILogger logger)
        {
            Console.WriteLine("LogViewerControl constructed");
            InitializeComponent();
            ViewModel = new LogViewerViewModel(_logFileWatcherService);
            DataContext = ViewModel;
        }
        public LogViewerControl() : this((ILogger)App.Container.Resolve(typeof(ILogger), null)) { }
        public void UpdateLogFiles(List<string> logFilePaths)
        {
            ViewModel.UpdateLogFiles(logFilePaths);
        }
    }
} 