using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using Todos.Client.Orchestrator.Services;

namespace Todos.Client.Orchestrator.ViewModels
{
    public class LogLineViewModel
    {
        public string FilePath { get; set; }
        public string Line { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Display => $"[{System.IO.Path.GetFileName(FilePath)}] {Line}";
    }

    public class LogViewerViewModel : INotifyPropertyChanged
    {
        private readonly LogFileWatcherService _logFileWatcherService;
        private string _searchText;
        public ObservableCollection<LogLineViewModel> LogLines { get; } = new ObservableCollection<LogLineViewModel>();
        public ICollectionView FilteredLogLines { get; }
        public ICommand ClearCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilteredLogLines.Refresh();
                }
            }
        }

        public LogViewerViewModel(LogFileWatcherService logFileWatcherService)
        {
            _logFileWatcherService = logFileWatcherService;
            _logFileWatcherService.LogLineReceived += OnLogLineReceived;
            FilteredLogLines = CollectionViewSource.GetDefaultView(LogLines);
            FilteredLogLines.Filter = FilterLogLine;
            ClearCommand = new RelayCommand(param => LogLines.Clear());
        }

        public void UpdateLogFiles(System.Collections.Generic.List<string> logFilePaths)
        {
            App.Current.Dispatcher.Invoke(() => LogLines.Clear());
            _logFileWatcherService.SetLogFiles(logFilePaths);
            _logFileWatcherService.ReloadAllLogLines();
        }

        private void OnLogLineReceived(object sender, LogLineEventArgs e)
        {
            DateTime? ts = null;
            string line = e.Line;
            int firstSpace = line.IndexOf(' ');
            if (firstSpace > 0)
            {
                string maybeDate = line.Substring(0, firstSpace);
                if (DateTime.TryParse(maybeDate, out DateTime dt))
                    ts = dt;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                LogLineViewModel logLine = new LogLineViewModel { FilePath = e.FilePath, Line = e.Line, Timestamp = ts };
                LogLines.Add(logLine);
                // Sort after add
                var sorted = LogLines.OrderBy(x => x.Timestamp ?? DateTime.MinValue).ThenBy(x => x.FilePath).ToList();
                LogLines.Clear();
                foreach (var item in sorted)
                    LogLines.Add(item);
            });
        }

        private bool FilterLogLine(object obj)
        {
            if (obj is LogLineViewModel line)
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                return line.Line.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
} 