using Microsoft.AspNet.SignalR;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ToDos.DotNet.Common;
using ToDos.Server.Common.Interfaces;
using ToDos.TaskSyncServer.Interfaces;
using ToDos.TaskSyncServer.Attributes;
using Serilog;
using System.Collections.Generic;

namespace ToDos.TaskSyncServer.Hubs
{
    [SignalRJwtAuthentication]
    public class TaskHub : Hub, ITaskHub
    {
        private readonly ITaskService _taskService;
        private readonly ILogger _logger;
        private static int _activeConnections = 0;

        public TaskHub(ITaskService taskService, ILogger logger)
        {
            _taskService = taskService;
            _logger = logger;
            
            // Subscribe to service events for real-time broadcasting
            _taskService.TaskAdded += OnTaskAdded;
            _taskService.TaskUpdated += OnTaskUpdated;
            _taskService.TaskDeleted += OnTaskDeleted;
            _taskService.TaskLocked += OnTaskLocked;
            _taskService.TaskUnlocked += OnTaskUnlocked;
        }

        private int GetCurrentUserId()
        {
            var userIdString = Context.Request.Environment["UserId"] as string;
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return 0; // Return 0 for invalid user ID
        }

        public async Task<TaskDTO> AddTask(TaskDTO task)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                task.UserId = userId; // Set the current user as the task owner
                
                _logger.Information("Adding task for user: {UserId}", userId);
                var result = await _taskService.AddTaskAsync(task);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in AddTask for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<TaskDTO> UpdateTask(TaskDTO task)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                _logger.Information("Updating task {TaskId} for user: {UserId}", task.Id, userId);
                
                var result = await _taskService.UpdateTaskAsync(task);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in UpdateTask for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<bool> DeleteTask(int taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                _logger.Information("Deleting task {TaskId} for user: {UserId}", taskId, userId);
                
                var result = await _taskService.DeleteTaskAsync(taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in DeleteTask for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<bool> SetTaskCompletion(int taskId, bool isCompleted)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                _logger.Information("Setting task completion {TaskId} to {IsCompleted} for user: {UserId}", taskId, isCompleted, userId);
                
                var result = await _taskService.SetTaskCompletionAsync(taskId, isCompleted);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in SetTaskCompletion for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<bool> LockTask(int taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                _logger.Information("Locking task {TaskId} for user: {UserId}", taskId, userId);
                
                var result = await _taskService.LockTaskAsync(taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in LockTask for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<bool> UnlockTask(int taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                _logger.Information("Unlocking task {TaskId} for user: {UserId}", taskId, userId);
                
                var result = await _taskService.UnlockTaskAsync(taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in UnlockTask for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<IEnumerable<TaskDTO>> GetUserTasks(int userId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var currentUserId = GetCurrentUserId();
                _logger.Information("Getting tasks for user: {UserId} (requested by: {CurrentUserId})", userId, currentUserId);
                
                // For now, allow users to get their own tasks
                // In a real app, you might want to add authorization checks
                if (userId != currentUserId)
                {
                    _logger.Warning("User {CurrentUserId} attempted to get tasks for user {UserId}", currentUserId, userId);
                    throw new UnauthorizedAccessException("You can only get your own tasks");
                }
                
                var tasks = await _taskService.GetUserTasksAsync(userId);
                stopwatch.Stop();
                return tasks;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in GetUserTasks for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        #region Event Handlers for Real-time Broadcasting

        private void OnTaskAdded(TaskDTO task)
        {
            try
            {
                // Broadcast only to the user who owns the task
                var groupName = $"User_{task.UserId}";
                Clients.Group(groupName).TaskAdded(task);
                _logger.Information("Broadcasted TaskAdded to group: {GroupName}", groupName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error broadcasting TaskAdded for task: {TaskId}", task.Id);
            }
        }

        private void OnTaskUpdated(TaskDTO task)
        {
            try
            {
                // Broadcast only to the user who owns the task
                var groupName = $"User_{task.UserId}";
                Clients.Group(groupName).TaskUpdated(task);
                _logger.Information("Broadcasted TaskUpdated to group: {GroupName}", groupName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error broadcasting TaskUpdated for task: {TaskId}", task.Id);
            }
        }

        private void OnTaskDeleted(int taskId)
        {
            try
            {
                // For task deletion, we need to get the task owner from the service
                // This is a simplified approach - in a real implementation, you might want to
                // pass the userId along with the taskId in the event
                var userId = GetTaskOwnerFromService(taskId);
                if (userId > 0)
                {
                    var groupName = $"User_{userId}";
                    Clients.Group(groupName).TaskDeleted(taskId);
                    _logger.Information("Broadcasted TaskDeleted to group: {GroupName}", groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error broadcasting TaskDeleted for task: {TaskId}", taskId);
            }
        }

        private void OnTaskLocked(int taskId)
        {
            try
            {
                // For task locking, we need to get the task owner from the service
                var userId = GetTaskOwnerFromService(taskId);
                if (userId > 0)
                {
                    var groupName = $"User_{userId}";
                    Clients.Group(groupName).TaskLocked(taskId);
                    _logger.Information("Broadcasted TaskLocked to group: {GroupName}", groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error broadcasting TaskLocked for task: {TaskId}", taskId);
            }
        }

        private void OnTaskUnlocked(int taskId)
        {
            try
            {
                // For task unlocking, we need to get the task owner from the service
                var userId = GetTaskOwnerFromService(taskId);
                if (userId > 0)
                {
                    var groupName = $"User_{userId}";
                    Clients.Group(groupName).TaskUnlocked(taskId);
                    _logger.Information("Broadcasted TaskUnlocked to group: {GroupName}", groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error broadcasting TaskUnlocked for task: {TaskId}", taskId);
            }
        }

        private int GetTaskOwnerFromService(int taskId)
        {
            try
            {
                // This is a simplified approach - in a real implementation, you might want to
                // cache task ownership or pass it along with the event
                var task = _taskService.GetTaskByIdAsync(taskId).GetAwaiter().GetResult();
                return task?.UserId ?? 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting task owner for task: {TaskId}", taskId);
                return 0;
            }
        }

        #endregion

        #region SignalR Lifecycle Methods

        public override Task OnConnected()
        {
            _activeConnections++;
            var userId = GetCurrentUserId();
            
            // Add the user to their own group for receiving broadcasts
            if (userId > 0)
            {
                var groupName = $"User_{userId}";
                Groups.Add(Context.ConnectionId, groupName);
                _logger.Information("User {UserId} connected and added to group: {GroupName}. Active connections: {ActiveConnections}", 
                    userId, groupName, _activeConnections);
            }
            
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _activeConnections = Math.Max(0, _activeConnections - 1);
            var userId = GetCurrentUserId();
            
            _logger.Information("User {UserId} disconnected. Active connections: {ActiveConnections}", 
                userId, _activeConnections);
            
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userId = GetCurrentUserId();
            
            // Re-add the user to their group on reconnection
            if (userId > 0)
            {
                var groupName = $"User_{userId}";
                Groups.Add(Context.ConnectionId, groupName);
                _logger.Information("User {UserId} reconnected and re-added to group: {GroupName}", userId, groupName);
            }
            
            return base.OnReconnected();
        }

        #endregion

        public static int GetActiveConnections() => _activeConnections;
    }
} 