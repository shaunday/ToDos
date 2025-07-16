using Serilog;
using System.Threading.Tasks;
using Todos.Client.SignalRClient;
using ToDos.DotNet.Common;
using System.Collections.Generic;
using System.Linq;
using ToDos.MockAuthService;
using System;
using System.Collections.Concurrent;

namespace ToDos.Clients.Simulator
{
    public class TaskSyncClientAdapter : IDisposable
    {
        private readonly SignalRTaskSyncClient _client;
        private readonly ILogger _logger;
        private readonly MockJwtAuthService _authService;
        private readonly HashSet<int> _myTaskIds = new HashSet<int>();
        private readonly object _lockObj = new object();
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
                _client.TaskAdded += t =>
                {
                    lock (_lockObj)
                    {
                        _logger.Information("[Event] TaskAdded: {TaskId}", t.Id);
                        _myTaskIds.Add(t.Id);
                        if (!_lockedTaskId.HasValue)
                        {
                            // Try to lock the task if not already locked
                            _logger.Information("[Event] Attempting to lock TaskId: {TaskId}", t.Id);
                            LockTask(_userId, t.Id).GetAwaiter().GetResult();
                        }
                    }
                };
                _client.TaskUpdated += t =>
                {
                    lock (_lockObj)
                    {
                        _logger.Information("[Event] TaskUpdated: {TaskId}", t.Id);
                    }
                };
                _client.TaskDeleted += id =>
                {
                    lock (_lockObj)
                    {
                        _logger.Information("[Event] TaskDeleted: {TaskId}", id);
                        _myTaskIds.Remove(id);
                        if (_lockedTaskId == id)
                        {
                            _logger.Information("[Event] Locked task was deleted, clearing lock state for TaskId: {TaskId}", id);
                            _lockedTaskId = null;
                        }
                    }
                };
                _client.TaskLocked += id =>
                {
                    lock (_lockObj)
                    {
                        _logger.Information("[Event] TaskLocked: {TaskId}", id);
                        if (_lockedTaskId == id)
                        {
                            _logger.Information("[Event] Already locked TaskId: {TaskId}", id);
                        }
                    }
                };
                _client.TaskUnlocked += id =>
                {
                    lock (_lockObj)
                    {
                        _logger.Information("[Event] TaskUnlocked: {TaskId}", id);
                        if (_lockedTaskId == id)
                        {
                            _logger.Information("[Event] Clearing lock state for TaskId: {TaskId}", id);
                            _lockedTaskId = null;
                        }
                    }
                };
            }
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();
        }

        public async Task AddTask(int userId)
        {
            var rand = new Random(Guid.NewGuid().GetHashCode());
            var priorities = Enum.GetValues(typeof(ToDos.DotNet.Common.TaskPriority));
            var task = new TaskDTO
            {
                Title = $"SimTask_{userId}_{rand.Next(1000, 10000)}",
                Description = "Simulated task",
                UserId = userId,
                Priority = (ToDos.DotNet.Common.TaskPriority)priorities.GetValue(rand.Next(priorities.Length)),
                IsCompleted = rand.Next(2) == 0
            };
            var result = await _client.AddTaskAsync(task);
            _logger.Information("AddTask sent for user {UserId}, result: {Result}", userId, result);
            if (result && task.Id != 0)
            {
                lock (_lockObj)
                {
                    _myTaskIds.Add(task.Id);
                }
            }
        }

        public async Task DeleteTask(int userId)
        {
            int? id = null;
            lock (_lockObj)
            {
                if (_myTaskIds.Count > 0)
                    id = _myTaskIds.LastOrDefault();
            }
            if (id == null || id == 0)
            {
                _logger.Information("No task to delete for user {UserId}", userId);
                return;
            }
            var result = await _client.DeleteTaskAsync(userId, id.Value);
            _logger.Information("DeleteTask sent for user {UserId}, taskId: {TaskId}, result: {Result}", userId, id, result);
            if (result)
            {
                lock (_lockObj)
                {
                    _myTaskIds.Remove(id.Value);
                }
            }
        }

        public async Task LockTask(int userId, int? taskId = null)
        {
            int? id = taskId;
            lock (_lockObj)
            {
                if (!id.HasValue && _myTaskIds.Count > 0)
                    id = _myTaskIds.LastOrDefault();
                if (_lockedTaskId.HasValue)
                {
                    _logger.Information("Already locked a task, skipping lock for TaskId: {TaskId}", id);
                    return;
                }
            }
            if (id == null || id == 0)
            {
                _logger.Information("No task to lock for user {UserId}", userId);
                return;
            }
            var result = await _client.LockTaskAsync(userId, id.Value);
            _logger.Information("LockTask sent for user {UserId}, taskId: {TaskId}, result: {Result}", userId, id, result);
            if (result)
            {
                lock (_lockObj)
                {
                    _lockedTaskId = id;
                }
            }
        }

        public async Task UnlockTask(int userId)
        {
            int? id;
            lock (_lockObj)
            {
                id = _lockedTaskId;
                if (!id.HasValue)
                {
                    _logger.Information("No locked task to unlock for user {UserId}", userId);
                    return;
                }
            }
            var result = await _client.UnlockTaskAsync(userId, id.Value);
            _logger.Information("UnlockTask sent for user {UserId}, taskId: {TaskId}, result: {Result}", userId, id, result);
            if (result)
            {
                lock (_lockObj)
                {
                    _lockedTaskId = null;
                }
            }
        }

        public async Task GetAll(int userId)
        {
            var tasks = await _client.GetUserTasksAsync(userId);
            _logger.Information("GetAllTasks for user {UserId}: {Count} tasks", userId, tasks == null ? 0 : tasks.Count());
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