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
#pragma warning disable CS0067
        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<int> TaskDeleted;
        public event Action<int> TaskLocked;
        public event Action<int> TaskUnlocked;
#pragma warning restore CS0067

        private string _jwtToken = string.Empty;
        public string ConnectionId { get; private set; }

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
            if (string.IsNullOrEmpty(ConnectionId))
                ConnectionId = Guid.NewGuid().ToString();
            // Simulate connection delay
            Task.Delay(500).ContinueWith(_ => {
                ConnectionStatus = ConnectionStatus.Connected;
                _logger?.Information($"MockTaskSyncClient: Connected. ConnectionId: {ConnectionId}");
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

        public Task<bool> AddTaskAsync(TaskDTO task)
        {
            // Simulate add logic
            return Task.FromResult(true);
        }

        public Task<bool> UpdateTaskAsync(TaskDTO task)
        {
            _logger?.Information("MockTaskSyncClient: TaskUpdated event for TaskId {TaskId}", task.Id);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteTaskAsync(int userId, int taskId)
        {
            _logger?.Information("MockTaskSyncClient: TaskDeleted event for TaskId {TaskId} for UserId {UserId}", taskId, userId);
            return Task.FromResult(true);
        }

        public Task<bool> LockTaskAsync(int taskId)
        {
            _logger?.Information("MockTaskSyncClient: TaskLocked event for TaskId {TaskId}", taskId);
            return Task.FromResult(true);
        }
        public Task<bool> UnlockTaskAsync(int taskId)
        {
            _logger?.Information("MockTaskSyncClient: TaskUnlocked event for TaskId {TaskId}", taskId);
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