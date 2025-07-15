using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Todos.Client.Orchestrator.Services
{
    public class LogLineEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public string Line { get; set; }
    }

    public class LogFileWatcherService : IDisposable
    {
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
        private readonly Dictionary<string, long> _filePositions = new Dictionary<string, long>();
        private readonly Dictionary<string, FileStream> _streams = new Dictionary<string, FileStream>();
        private readonly SynchronizationContext _syncContext;
        private List<string> _logFiles = new List<string>();
        private bool _disposed;

        public event EventHandler<LogLineEventArgs> LogLineReceived;

        public LogFileWatcherService()
        {
            _syncContext = SynchronizationContext.Current;
        }

        public void SetLogFiles(List<string> logFilePaths)
        {
            _logFiles = logFilePaths.Distinct().ToList();
            UpdateWatchedFiles();
        }

        private void UpdateWatchedFiles()
        {
            var currentFiles = new HashSet<string>(_logFiles);
            var watchedFiles = _watchers.Keys.ToList();
            // Remove watchers for files no longer in the list
            foreach (var file in watchedFiles)
            {
                if (!currentFiles.Contains(file))
                {
                    _watchers[file].Dispose();
                    _watchers.Remove(file);
                    if (_streams.ContainsKey(file))
                    {
                        _streams[file].Dispose();
                        _streams.Remove(file);
                    }
                    _filePositions.Remove(file);
                }
            }
            // Add watchers for new files
            foreach (var file in currentFiles)
            {
                if (!_watchers.ContainsKey(file))
                {
                    StartWatching(file);
                }
            }
        }

        private void StartWatching(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            var file = Path.GetFileName(filePath);
            if (!Directory.Exists(dir) || string.IsNullOrEmpty(file)) return;
            var watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            watcher.Changed += (s, e) => OnFileChanged(filePath);
            watcher.EnableRaisingEvents = true;
            _watchers[filePath] = watcher;
            _filePositions[filePath] = 0;
            // Open stream for tailing
            if (File.Exists(filePath))
            {
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _streams[filePath] = stream;
                _filePositions[filePath] = stream.Length;
            }
        }

        private void OnFileChanged(string filePath)
        {
            Task.Run(() =>
            {
                if (_disposed) return;
                if (!_streams.ContainsKey(filePath))
                {
                    if (!File.Exists(filePath)) return;
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    _streams[filePath] = fileStream;
                    _filePositions[filePath] = 0;
                }
                FileStream stream = _streams[filePath];
                lock (stream)
                {
                    stream.Seek(_filePositions[filePath], SeekOrigin.Begin);
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            LogLineEventArgs args = new LogLineEventArgs { FilePath = filePath, Line = line };
                            if (_syncContext != null)
                                _syncContext.Post(_ => LogLineReceived?.Invoke(this, args), null);
                            else
                                LogLineReceived?.Invoke(this, args);
                        }
                        _filePositions[filePath] = stream.Position;
                    }
                }
            });
        }

        public void ReloadAllLogLines()
        {
            if (_logFiles == null) return;
            var allLines = new List<Tuple<DateTime?, string, string>>();
            foreach (var filePath in _logFiles)
            {
                if (!File.Exists(filePath)) continue;
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, 1024, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        DateTime? ts = null;
                        var firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                        {
                            var maybeDate = line.Substring(0, firstSpace);
                            if (DateTime.TryParse(maybeDate, out var dt))
                                ts = dt;
                        }
                        allLines.Add(new Tuple<DateTime?, string, string>(ts, filePath, line));
                    }
                }
            }
            // Sort by timestamp if available, else by file, then by line
            foreach (var entry in allLines.OrderBy(x => x.Item1 ?? DateTime.MinValue).ThenBy(x => x.Item2))
            {
                var args = new LogLineEventArgs { FilePath = entry.Item2, Line = entry.Item3 };
                if (_syncContext != null)
                    _syncContext.Post(_ => LogLineReceived?.Invoke(this, args), null);
                else
                    LogLineReceived?.Invoke(this, args);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (var watcher in _watchers.Values)
                watcher.Dispose();
            foreach (var stream in _streams.Values)
                stream.Dispose();
        }
    }
} 