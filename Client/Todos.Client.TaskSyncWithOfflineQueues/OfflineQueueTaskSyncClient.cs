using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;
using Todos.Client.Common.Interfaces;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Client.TaskSyncWithOfflineQueues
{
    public class TaskSyncWithOfflineQueues : ITaskSyncClient
    {
        private readonly ITaskSyncClient _innerClient;
        private readonly IOfflineQueueService _queueService;
        private bool _isOnline = true;

        public TaskSyncWithOfflineQueues(ITaskSyncClient innerClient, IOfflineQueueService queueService)
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
        public event Action<TaskDTO> TaskAdded { add { _innerClient.TaskAdded += value; } remove { _innerClient.TaskAdded -= value; } }
        public event Action<TaskDTO> TaskUpdated { add { _innerClient.TaskUpdated += value; } remove { _innerClient.TaskUpdated -= value; } }
        public event Action<int> TaskDeleted { add { _innerClient.TaskDeleted += value; } remove { _innerClient.TaskDeleted -= value; } }
        public event Action<int> TaskLocked { add { _innerClient.TaskLocked += value; } remove { _innerClient.TaskLocked -= value; } }
        public event Action<int> TaskUnlocked { add { _innerClient.TaskUnlocked += value; } remove { _innerClient.TaskUnlocked -= value; } }

        public void SetJwtToken(string jwt) => _innerClient.SetJwtToken(jwt);
        public string GetJwtToken() => _innerClient.GetJwtToken();

        public Task ConnectAsync() => _innerClient.ConnectAsync();
        public Task DisconnectAsync() => _innerClient.DisconnectAsync();

        public async Task<TaskDTO> AddTaskAsync(TaskDTO task)
        {
            if (_isOnline)
                return await _innerClient.AddTaskAsync(task);
            else
            {
                var action = new PendingAction { ActionType = "Add", Task = task, UserId = task.UserId, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return task;
            }
        }

        public async Task<TaskDTO> UpdateTaskAsync(TaskDTO task)
        {
            if (_isOnline)
                return await _innerClient.UpdateTaskAsync(task);
            else
            {
                var action = new PendingAction { ActionType = "Update", Task = task, UserId = task.UserId, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return task;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            if (_isOnline)
                return await _innerClient.DeleteTaskAsync(taskId);
            else
            {
                var action = new PendingAction { ActionType = "Delete", Task = new TaskDTO { Id = taskId }, Timestamp = DateTime.UtcNow };
                _queueService.Enqueue(action);
                return true;
            }
        }

        public async Task<bool> SetTaskCompletionAsync(int taskId, bool isCompleted)
        {
            if (_isOnline)
                return await _innerClient.SetTaskCompletionAsync(taskId, isCompleted);
            else
            {
                var action = new PendingAction { ActionType = "SetCompletion", Task = new TaskDTO { Id = taskId, IsCompleted = isCompleted }, Timestamp = DateTime.UtcNow };
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
                        await _innerClient.DeleteTaskAsync(action.Task.Id);
                        break;
                    case "SetCompletion":
                        await _innerClient.SetTaskCompletionAsync(action.Task.Id, action.Task.IsCompleted);
                        break;
                    case "Lock":
                        await _innerClient.LockTaskAsync(action.Task.Id);
                        break;
                    case "Unlock":
                        await _innerClient.UnlockTaskAsync(action.Task.Id);
                        break;
                }
                _queueService.Remove(action);
            }
        }
    }
} 