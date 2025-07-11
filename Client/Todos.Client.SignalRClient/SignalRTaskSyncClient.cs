using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using DotNetEnv;

namespace Todos.Client.SignalRClient
{
    public class SignalRTaskSyncClient : ITaskSyncClient
    {
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private readonly string _hubUrl;

        public bool IsConnected => _hubConnection?.State == ConnectionState.Connected;

        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<Guid> TaskDeleted;
        public event Action<Guid> TaskLocked;
        public event Action<Guid> TaskUnlocked;

        public SignalRTaskSyncClient()
        {
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            Env.Load(envPath);
            _hubUrl = Environment.GetEnvironmentVariable(SignalRGlobals.URL_String_Identifier);

            if (string.IsNullOrWhiteSpace(_hubUrl))
                throw new InvalidOperationException("SIGNALR_SERVER_URL is not set in .env file.");
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection != null)
            {
                await DisconnectAsync();
            }

            _hubConnection = new HubConnection(_hubUrl);
            _hubProxy = _hubConnection.CreateHubProxy("TaskHub");

            _hubProxy.On<TaskDTO>(SignalRGlobals.TaskAdded, task => TaskAdded?.Invoke(task));
            _hubProxy.On<TaskDTO>(SignalRGlobals.TaskUpdated, task => TaskUpdated?.Invoke(task));
            _hubProxy.On<Guid>(SignalRGlobals.TaskDeleted, id => TaskDeleted?.Invoke(id));
            _hubProxy.On<Guid>(SignalRGlobals.TaskLocked, id => TaskLocked?.Invoke(id));
            _hubProxy.On<Guid>(SignalRGlobals.TaskUnlocked, id => TaskUnlocked?.Invoke(id));

            await _hubConnection.Start();
        }

        public Task DisconnectAsync()
        {
            return Task.Run(() =>
            {
                if (_hubConnection != null)
                {
                    _hubConnection.Stop();
                    _hubConnection.Dispose();
                    _hubConnection = null;
                }
            });
        }

        public Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            EnsureConnected();
            return _hubProxy.Invoke<IEnumerable<TaskDTO>>(SignalRGlobals.GetAllTasks);
        }

        public Task<TaskDTO> AddTaskAsync(TaskDTO task)
        {
            EnsureConnected();
            return _hubProxy.Invoke<TaskDTO>(SignalRGlobals.AddTask, task);
        }

        public Task<TaskDTO> UpdateTaskAsync(TaskDTO task)
        {
            EnsureConnected();
            return _hubProxy.Invoke<TaskDTO>(SignalRGlobals.UpdateTask, task);
        }

        public Task<bool> DeleteTaskAsync(Guid taskId)
        {
            EnsureConnected();
            return _hubProxy.Invoke<bool>(SignalRGlobals.DeleteTask, taskId);
        }

        public Task<bool> SetTaskCompletionAsync(Guid taskId, bool isCompleted)
        {
            EnsureConnected();
            return _hubProxy.Invoke<bool>(SignalRGlobals.SetTaskCompletion, taskId, isCompleted);
        }

        public Task<bool> LockTaskAsync(Guid taskId)
        {
            EnsureConnected();
            return _hubProxy.Invoke<bool>(SignalRGlobals.LockTask, taskId);
        }

        public Task<bool> UnlockTaskAsync(Guid taskId)
        {
            EnsureConnected();
            return _hubProxy.Invoke<bool>(SignalRGlobals.UnlockTask, taskId);
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to SignalR hub.");
        }
    }
}
