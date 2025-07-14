using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using static Todos.Client.Common.TypesGlobal;
using Serilog;

namespace Todos.Client.MockTaskSyncClient
{
    public class MockTaskSyncClient : ITaskSyncClient
    {
        private readonly ILogger _logger;
        private ConnectionStatus _connectionStatus = ConnectionStatus.Connected;
        
        public MockTaskSyncClient(ILogger logger = null)
        {
            _logger = logger ?? Log.Logger;
        }
        
        public ConnectionStatus ConnectionStatus 
        { 
            get => _connectionStatus;
            private set
            {
                if (_connectionStatus != value)
                {
                    _logger?.Information("ConnectionStatus changed from {OldStatus} to {NewStatus}", _connectionStatus, value);
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
            _logger?.Information("JWT token set in MockTaskSyncClient");
        }

        public string GetJwtToken()
        {
            return _jwtToken;
        }

        public Task ConnectAsync()
        {
            _logger?.Information("MockTaskSyncClient: Connecting...");
            ConnectionStatus = ConnectionStatus.Connecting;
            // Simulate connection delay
            Task.Delay(500).ContinueWith(_ => {
                ConnectionStatus = ConnectionStatus.Connected;
                _logger?.Information("MockTaskSyncClient: Connected.");
            });
            return Task.CompletedTask;
        }
        
        public Task DisconnectAsync()
        {
            _logger?.Information("MockTaskSyncClient: Disconnecting...");
            ConnectionStatus = ConnectionStatus.Disconnected;
            _logger?.Information("MockTaskSyncClient: Disconnected.");
            return Task.CompletedTask;
        }

        public Task<TaskDTO> AddTaskAsync(TaskDTO task)
        {
            task.Id = new Random().Next(1, 10000); // Simple random ID for mock
            _logger?.Information("MockTaskSyncClient: TaskAdded event for TaskId {TaskId}", task.Id);
            TaskAdded?.Invoke(task);
            return Task.FromResult(task);
        }

        public Task<TaskDTO> UpdateTaskAsync(TaskDTO task)
        {
            _logger?.Information("MockTaskSyncClient: TaskUpdated event for TaskId {TaskId}", task.Id);
            TaskUpdated?.Invoke(task);
            return Task.FromResult(task);
        }

        public Task<bool> DeleteTaskAsync(int taskId)
        {
            _logger?.Information("MockTaskSyncClient: TaskDeleted event for TaskId {TaskId}", taskId);
            TaskDeleted?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<bool> SetTaskCompletionAsync(int taskId, bool isCompleted)
        {
            _logger?.Information("MockTaskSyncClient: SetTaskCompletionAsync for TaskId {TaskId}, IsCompleted: {IsCompleted}", taskId, isCompleted);
            // For mock, just return true
            return Task.FromResult(true);
        }

        public Task<bool> LockTaskAsync(int taskId)
        {
            _logger?.Information("MockTaskSyncClient: TaskLocked event for TaskId {TaskId}", taskId);
            TaskLocked?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<bool> UnlockTaskAsync(int taskId)
        {
            _logger?.Information("MockTaskSyncClient: TaskUnlocked event for TaskId {TaskId}", taskId);
            TaskUnlocked?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId)
        {
            _logger?.Information("MockTaskSyncClient: GetUserTasksAsync for UserId {UserId}", userId);
            // Return mock tasks for the specified user using the shared mock data
            var allMockTasks = MockTaskData.GetSampleTasks();
            var userTasks = allMockTasks.Where(t => t.UserId == userId).ToList();
            
            return Task.FromResult<IEnumerable<TaskDTO>>(userTasks);
        }
    }
} 