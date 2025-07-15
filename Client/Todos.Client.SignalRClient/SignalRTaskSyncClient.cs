using DotNetEnv;
using Microsoft.AspNet.SignalR.Client;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using ToDos.DotNet.Common.SignalR;
using static Todos.Client.Common.TypesGlobal;
using System.IO;
using System.Text;

namespace Todos.Client.SignalRClient
{
    public class SignalRTaskSyncClient : ITaskSyncClient
    {
        #region Fields, Properties & Events

        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private readonly string _hubUrl;
        private readonly ILogger _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private string _jwtToken;

        private int _reconnectAttempts = 0;
        private const int MaxReconnectAttempts = 5;

        public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Disconnected;
        public event Action<ConnectionStatus> ConnectionStatusChanged;

        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<int> TaskDeleted;
        public event Action<int> TaskLocked;
        public event Action<int> TaskUnlocked;

        public string ConnectionId { get; private set; }

        // Queue to hold pending hub method calls when disconnected
        private readonly ConcurrentQueue<Func<Task>> _pendingCalls = new ConcurrentQueue<Func<Task>>();

        private readonly object _connectionLock = new object();

        #endregion

        #region Constructor

        public SignalRTaskSyncClient(ILogger logger)
        {
            _logger = logger;
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            Env.Load(envPath);
            _hubUrl = Environment.GetEnvironmentVariable(SignalRGlobals.URL_String_Identifier);

            if (string.IsNullOrWhiteSpace(_hubUrl))
            {
                string errorMsg = $"{SignalRGlobals.URL_String_Identifier} is not set in .env file.";
                _logger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.Warning("Retry {RetryCount} after {TimeSpan} due to: {Exception}", retryCount, timeSpan, exception.Message);
                    });
        }

        #endregion

        #region JWT Token Management

        public void SetJwtToken(string jwt)
        {
            _jwtToken = jwt;
            _logger.Information("JWT token set for SignalR client");
        }

        public string GetJwtToken()
        {
            return _jwtToken;
        }

        #endregion

        #region Connect / Disconnect

        public async Task ConnectAsync()
        {
            lock (_connectionLock)
            {
                if (_hubConnection != null)
                {
                    _logger.Information("Disconnecting existing SignalR connection...");
                    DisconnectAsync().GetAwaiter().GetResult(); // sync call inside lock is safe here
                }
            }

            SetConnectionStatus(ConnectionStatus.Connecting);

            try
            {
                _logger.Information("Connecting to SignalR hub at {HubUrl}...", _hubUrl);
                
                // Create connection with query string parameters
                var queryString = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    queryString["token"] = _jwtToken;
                    _logger.Information("JWT token added to SignalR connection");
                }
                else
                {
                    _logger.Warning("No JWT token available for SignalR connection");
                }

                _hubConnection = new HubConnection(_hubUrl, queryString);

                SetupReconnect();

                _hubProxy = _hubConnection.CreateHubProxy("TaskHub");

                _hubProxy.On<TaskDTO>(SignalRGlobals.TaskAdded, task => SafeInvoke(() => TaskAdded?.Invoke(task), nameof(TaskAdded)));
                _hubProxy.On<TaskDTO>(SignalRGlobals.TaskUpdated, task => SafeInvoke(() => TaskUpdated?.Invoke(task), nameof(TaskUpdated)));
                _hubProxy.On<int>(SignalRGlobals.TaskDeleted, id => SafeInvoke(() => TaskDeleted?.Invoke(id), nameof(TaskDeleted)));
                _hubProxy.On<int>(SignalRGlobals.TaskLocked, id => SafeInvoke(() => TaskLocked?.Invoke(id), nameof(TaskLocked)));
                _hubProxy.On<int>(SignalRGlobals.TaskUnlocked, id => SafeInvoke(() => TaskUnlocked?.Invoke(id), nameof(TaskUnlocked)));

                await _hubConnection.Start();

                // Retrieve and set ConnectionId from server
                if (string.IsNullOrEmpty(ConnectionId))
                {
                    ConnectionId = await _hubProxy.Invoke<string>("GetConnectionId");
                    _logger.Information($"SignalR connection established. ConnectionId: {ConnectionId}");
                }
                else
                {
                    _logger.Information($"SignalR connection established. Existing ConnectionId: {ConnectionId}");
                }
                SetConnectionStatus(ConnectionStatus.Connected);

                // Drain pending calls queued while disconnected
                await DrainPendingCallsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to connect to SignalR hub.");
                SetConnectionStatus(ConnectionStatus.Failed);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    _logger.Information("Stopping SignalR connection...");
                    _hubConnection.Stop();
                    _hubConnection.Dispose();
                    _hubConnection = null;
                    _logger.Information("SignalR connection stopped.");
                    SetConnectionStatus(ConnectionStatus.Disconnected);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while disconnecting SignalR connection.");
            }

            await Task.CompletedTask;
        }

        private void SetupReconnect()
        {
            _hubConnection.Closed += async () =>
            {
                _logger.Warning("SignalR connection closed. Attempting reconnect...");
                SetConnectionStatus(ConnectionStatus.Reconnecting);

                while (_reconnectAttempts < MaxReconnectAttempts)
                {
                    try
                    {
                        _reconnectAttempts++;
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, _reconnectAttempts));
                        _logger.Information("Waiting {Delay} before reconnect attempt {Attempt}", delay, _reconnectAttempts);
                        await Task.Delay(delay);

                        // Recreate connection with JWT token for reconnection
                        var queryString = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(_jwtToken))
                        {
                            queryString["token"] = _jwtToken;
                        }

                        _hubConnection = new HubConnection(_hubUrl, queryString);

                        // Re-setup event handlers
                        _hubProxy = _hubConnection.CreateHubProxy("TaskHub");
                        _hubProxy.On<TaskDTO>(SignalRGlobals.TaskAdded, task => SafeInvoke(() => TaskAdded?.Invoke(task), nameof(TaskAdded)));
                        _hubProxy.On<TaskDTO>(SignalRGlobals.TaskUpdated, task => SafeInvoke(() => TaskUpdated?.Invoke(task), nameof(TaskUpdated)));
                        _hubProxy.On<int>(SignalRGlobals.TaskDeleted, id => SafeInvoke(() => TaskDeleted?.Invoke(id), nameof(TaskDeleted)));
                        _hubProxy.On<int>(SignalRGlobals.TaskLocked, id => SafeInvoke(() => TaskLocked?.Invoke(id), nameof(TaskLocked)));
                        _hubProxy.On<int>(SignalRGlobals.TaskUnlocked, id => SafeInvoke(() => TaskUnlocked?.Invoke(id), nameof(TaskUnlocked)));

                        await _hubConnection.Start();
                        _logger.Information("SignalR reconnected.");
                        SetConnectionStatus(ConnectionStatus.Connected);
                        _reconnectAttempts = 0;

                        // Drain pending calls if any
                        await DrainPendingCallsAsync();

                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Reconnect attempt {Attempt} failed.", _reconnectAttempts);
                    }
                }

                if (_reconnectAttempts >= MaxReconnectAttempts)
                {
                    _logger.Error("Max reconnect attempts reached. Giving up.");
                    SetConnectionStatus(ConnectionStatus.Failed);
                }
            };
        }

        private void SetConnectionStatus(ConnectionStatus status)
        {
            if (ConnectionStatus != status)
            {
                ConnectionStatus = status;
                ConnectionStatusChanged?.Invoke(status);
            }
        }

        private async Task DrainPendingCallsAsync()
        {
            while (_pendingCalls.TryDequeue(out var call))
            {
                try
                {
                    await call();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error while executing queued SignalR call.");
                }
            }
        }

        #endregion

        #region Public API Methods


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> AddTaskAsync(TaskDTO task)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Simulate add logic (call server, etc.)
            return true;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> UpdateTaskAsync(TaskDTO task)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Simulate update logic (call server, etc.)
            return true;
        }

        public Task<bool> DeleteTaskAsync(int userId, int taskId)
        {
            return InvokeWithRetrySafeAsync<bool>(SignalRGlobals.DeleteTask, userId, taskId);
        }

        public Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId) =>
            InvokeWithRetrySafeAsync<IEnumerable<TaskDTO>>(SignalRGlobals.GetUserTasks, userId);

        public Task<bool> LockTaskAsync(int taskId)
        {
            return InvokeWithRetrySafeAsync<bool>(SignalRGlobals.LockTask, taskId);
        }
        public Task<bool> UnlockTaskAsync(int taskId)
        {
            return InvokeWithRetrySafeAsync<bool>(SignalRGlobals.UnlockTask, taskId);
        }

        #endregion

        #region Helpers

        private Task<T> InvokeWithRetrySafeAsync<T>(string methodName, params object[] args)
        {
            if (ConnectionStatus != ConnectionStatus.Connected)
            {
                _logger.Warning("Not connected to SignalR hub. Queuing {MethodName} call.", methodName);

                // Queue call for later, return default(T) wrapped in Task immediately to avoid crashing
                var tcs = new TaskCompletionSource<T>();

                _pendingCalls.Enqueue(async () =>
                {
                    try
                    {
                        var result = await InvokeWithRetryAsync<T>(methodName, args);
                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });

                return tcs.Task;
            }

            return InvokeWithRetryAsync<T>(methodName, args);
        }

        private async Task<T> InvokeWithRetryAsync<T>(string methodName, params object[] args)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.Information("Invoking {MethodName} on SignalR hub...", methodName);
                return await _hubProxy.Invoke<T>(methodName, args);
            });
        }

        private void SafeInvoke(Action action, string context)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in event handler for {Context}", context);
            }
        }

        #endregion
    }
}