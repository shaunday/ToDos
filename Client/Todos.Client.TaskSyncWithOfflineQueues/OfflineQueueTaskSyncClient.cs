using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;
using Todos.Client.Common.Interfaces;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Client.TaskSyncWithOfflineQueues
{
    public class OfflineQueueTaskSyncClient : ITaskSyncClient
    {
        private readonly ITaskSyncClient _innerClient;
        private readonly IOfflineQueueService _queueService;
        private bool _isOnline = true;

        public OfflineQueueTaskSyncClient(ITaskSyncClient innerClient, IOfflineQueueService queueService)
        {
            _innerClient = innerClient;
            _queueService = queueService;
            _innerClient.ConnectionStatusChanged += status =>
            {
                _isOnline = status == ConnectionStatus.Connected;
                ConnectionStatusChanged?.Invoke(status);
                if (_isOnline)
                {
                    // On reconnect, replay queued actions
                    ReplayPendingActions();
                }
            };
        }

        public ConnectionStatus ConnectionStatus => _innerClient.ConnectionStatus;
        public event Action<ConnectionStatus> ConnectionStatusChanged;

        // Event forwarding: these simply pass through to the inner client.
        // This is the cleanest way to expose the same events if you do not need custom logic.
        // If you want to add custom logic (e.g., filtering, transformation),
        // use a private backing event and subscribe to _innerClient events manually.
        public event Action<TaskDTO> TaskAdded { add { _innerClient.TaskAdded += value; } remove { _innerClient.TaskAdded -= value; } }
        public event Action<TaskDTO> TaskUpdated { add { _innerClient.TaskUpdated += value; } remove { _innerClient.TaskUpdated -= value; } }
        public event Action<int> TaskDeleted { add { _innerClient.TaskDeleted += value; } remove { _innerClient.TaskDeleted -= value; } }
        public event Action<int> TaskLocked { add { _innerClient.TaskLocked += value; } remove { _innerClient.TaskLocked -= value; } }
        public event Action<int> TaskUnlocked { add { _innerClient.TaskUnlocked += value; } remove { _innerClient.TaskUnlocked -= value; } }

        public void SetJwtToken(string jwt) => _innerClient.SetJwtToken(jwt);
        public string GetJwtToken() => _innerClient.GetJwtToken();

        public Task ConnectAsync() => _innerClient.ConnectAsync();
        public Task DisconnectAsync() => _innerClient.DisconnectAsync();

        public async Task<bool> AddTaskAsync(TaskDTO task)
        {
            if (_isOnline)
                return await _innerClient.AddTaskAsync(task);
            else
            {
                var action = new PendingAction { ActionType = "Add", Task = task, UserId = task.UserId, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return true;
            }
        }

        public async Task<bool> UpdateTaskAsync(TaskDTO task)
        {
            if (_isOnline)
                return await _innerClient.UpdateTaskAsync(task);
            else
            {
                var action = new PendingAction { ActionType = "Update", Task = task, UserId = task.UserId, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return true;
            }
        }

        public async Task<bool> DeleteTaskAsync(int userId, int taskId)
        {
            if (_isOnline)
                return await _innerClient.DeleteTaskAsync(userId, taskId);
            else
            {
                var action = new PendingAction { ActionType = "Delete", Task = new TaskDTO { Id = taskId }, UserId = userId, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return true;
            }
        }

        public async Task<bool> LockTaskAsync(int taskId)
        {
            if (_isOnline)
                return await _innerClient.LockTaskAsync(taskId);
            else
            {
                var action = new PendingAction { ActionType = "Lock", Task = new TaskDTO { Id = taskId }, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return true;
            }
        }
        public async Task<bool> UnlockTaskAsync(int taskId)
        {
            if (_isOnline)
                return await _innerClient.UnlockTaskAsync(taskId);
            else
            {
                var action = new PendingAction { ActionType = "Unlock", Task = new TaskDTO { Id = taskId }, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return true;
            }
        }

        public async Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId)
        {
            if (_isOnline)
                return await _innerClient.GetUserTasksAsync(userId);
            else
                return new List<TaskDTO>(); // Optionally return cached tasks
        }

        private async void ReplayPendingActions()
        {
            var actions = _queueService.GetAll();
            foreach (var action in actions)
            {
                switch (action.ActionType)
                {
                    case "Add":
                        await _innerClient.AddTaskAsync(action.Task);
                        break;
                    case "Update":
                        await _innerClient.UpdateTaskAsync(action.Task);
                        break;
                    case "Delete":
                        await _innerClient.DeleteTaskAsync(action.UserId, action.Task.Id);
                        break;
                }
                _queueService.Remove(action);
            }
        }
    }
} 