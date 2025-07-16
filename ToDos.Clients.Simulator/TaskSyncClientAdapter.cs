using Serilog;
using System.Threading.Tasks;
using Todos.Client.SignalRClient;
using ToDos.DotNet.Common;
using System.Collections.Generic;
using System.Linq;
using ToDos.MockAuthService;
using System;

namespace ToDos.Clients.Simulator
{
    public class TaskSyncClientAdapter : IDisposable
    {
        private readonly SignalRTaskSyncClient _client;
        private readonly ILogger _logger;
        private readonly MockJwtAuthService _authService;
        private readonly List<int> _addedTaskIds = new List<int>();
        private int? _lockedTaskId = null;
        private bool _signToEvents = false;
        private int _userId;

        public TaskSyncClientAdapter(ILogger logger, MockJwtAuthService authService)
        {
            _logger = logger;
            _authService = authService;
            _client = new SignalRTaskSyncClient(logger);
        }

        public void Configure(int userId, bool signToEvents)
        {
            _userId = userId;
            _signToEvents = signToEvents;
            var mockToken = _authService.GenerateToken(userId);
            _client.SetJwtToken(mockToken);
            if (_signToEvents)
            {
                _client.TaskAdded += t => _logger.Info("[Event] TaskAdded: {TaskId}", t.Id);
                _client.TaskUpdated += t => _logger.Info("[Event] TaskUpdated: {TaskId}", t.Id);
                _client.TaskDeleted += id => _logger.Info("[Event] TaskDeleted: {TaskId}", id);
                _client.TaskLocked += id => _logger.Info("[Event] TaskLocked: {TaskId}", id);
                _client.TaskUnlocked += id => _logger.Info("[Event] TaskUnlocked: {TaskId}", id);
            }
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();
        }

        public async Task AddTask(int userId)
        {
            var task = new TaskDTO
            {
                Title = $"SimTask_{userId}_{Guid.NewGuid()}" ,
                Description = "Simulated task",
                UserId = userId
            };
            var result = await _client.AddTaskAsync(task);
            _logger.Info("AddTask sent for user {UserId}, result: {Result}", userId, result);
            if (result && task.Id != 0)
                _addedTaskIds.Add(task.Id);
        }

        public async Task DeleteTask(int userId)
        {
            int? id = _addedTaskIds.LastOrDefault();
            if (id == null || id == 0)
            {
                _logger.Info("No task to delete for user {UserId}", userId);
                return;
            }
            var result = await _client.DeleteTaskAsync(userId, id.Value);
            _logger.Info("DeleteTask sent for user {UserId}, taskId: {TaskId}, result: {Result}", userId, id, result);
            if (result)
                _addedTaskIds.Remove(id.Value);
        }

        public async Task LockTask(int userId)
        {
            int? id = _addedTaskIds.LastOrDefault();
            if (id == null || id == 0)
            {
                _logger.Info("No task to lock for user {UserId}", userId);
                return;
            }
            var result = await _client.LockTaskAsync(userId, id.Value);
            _logger.Info("LockTask sent for user {UserId}, taskId: {TaskId}, result: {Result}", userId, id, result);
            if (result)
                _lockedTaskId = id;
        }

        public async Task UnlockTask(int userId)
        {
            int? id = _lockedTaskId;
            if (id == null || id == 0)
            {
                _logger.Info("No locked task to unlock for user {UserId}", userId);
                return;
            }
            var result = await _client.UnlockTaskAsync(userId, id.Value);
            _logger.Info("UnlockTask sent for user {UserId}, taskId: {TaskId}, result: {Result}", userId, id, result);
            if (result)
                _lockedTaskId = null;
        }

        public async Task GetAll(int userId)
        {
            var tasks = await _client.GetUserTasksAsync(userId);
            _logger.Info("GetAllTasks for user {UserId}: {Count} tasks", userId, tasks == null ? 0 : tasks.Count());
        }

        public async Task CleanupOnExit()
        {
            if (_lockedTaskId.HasValue)
            {
                await UnlockTask(_userId);
            }
        }

        public void Dispose()
        {
            CleanupOnExit().GetAwaiter().GetResult();
        }
    }
} 