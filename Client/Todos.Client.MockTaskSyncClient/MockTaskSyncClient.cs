using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Client.MockTaskSyncClient
{
    public class MockTaskSyncClient : ITaskSyncClient
    {
        private ConnectionStatus _connectionStatus = ConnectionStatus.Connected;
        
        public ConnectionStatus ConnectionStatus 
        { 
            get => _connectionStatus;
            private set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    ConnectionStatusChanged?.Invoke(_connectionStatus);
                }
            }
        }
        
        public event Action<ConnectionStatus> ConnectionStatusChanged;
        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<int> TaskDeleted;
        public event Action<int> TaskLocked;
        public event Action<int> TaskUnlocked;

        private string _jwtToken = string.Empty;

        public void SetJwtToken(string jwt)
        {
            _jwtToken = jwt;
        }

        public string GetJwtToken()
        {
            return _jwtToken;
        }

        public Task ConnectAsync()
        {
            ConnectionStatus = ConnectionStatus.Connecting;
            // Simulate connection delay
            Task.Delay(500).ContinueWith(_ => ConnectionStatus = ConnectionStatus.Connected);
            return Task.CompletedTask;
        }
        
        public Task DisconnectAsync()
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            return Task.CompletedTask;
        }

        public Task<TaskDTO> AddTaskAsync(TaskDTO task)
        {
            task.Id = new Random().Next(1, 10000); // Simple random ID for mock
            TaskAdded?.Invoke(task);
            return Task.FromResult(task);
        }

        public Task<TaskDTO> UpdateTaskAsync(TaskDTO task)
        {
            TaskUpdated?.Invoke(task);
            return Task.FromResult(task);
        }

        public Task<bool> DeleteTaskAsync(int taskId)
        {
            TaskDeleted?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<bool> SetTaskCompletionAsync(int taskId, bool isCompleted)
        {
            // For mock, just return true
            return Task.FromResult(true);
        }

        public Task<bool> LockTaskAsync(int taskId)
        {
            TaskLocked?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<bool> UnlockTaskAsync(int taskId)
        {
            TaskUnlocked?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId)
        {
            // Return empty list for mock - in real implementation, this would return user's tasks
            return Task.FromResult<IEnumerable<TaskDTO>>(new List<TaskDTO>());
        }
    }
} 