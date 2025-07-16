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
using ToDos.DotNet.Common.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;

namespace ToDos.TaskSyncServer.Hubs
{
    [SignalRJwtAuthentication]
    [HubName("TaskHub")]
    public class TaskHub : Hub, ITaskHub
    {
        private readonly ITaskService _taskService;
        private readonly ILogger _logger;
        private static int _activeConnections = 0;

        // Track group membership: group name -> set of connection IDs
        private static readonly ConcurrentDictionary<string, HashSet<string>> GroupMembers = new ConcurrentDictionary<string, HashSet<string>>();
        private static readonly object GroupLock = new object();

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
            return -1; 
        }

        public async Task<bool> AddTask(TaskDTO task)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                task.UserId = userId; // Set the current user as the task owner
                
                _logger.Information("Adding task for user: {UserId}", userId);
                var result = await _taskService.AddTaskAsync(task);
                // Broadcast to all except sender
                BroadcastTaskAdded(result);
                stopwatch.Stop();
                return true; // Return success if we got here
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in AddTask for user: {UserId}", GetCurrentUserId());
                throw;
            }
        }

        public async Task<bool> UpdateTask(TaskDTO task)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userId = GetCurrentUserId();
                _logger.Information("Updating task {TaskId} for user: {UserId}", task.Id, userId);
                var result = await _taskService.UpdateTaskAsync(task);
                if (result)
                {
                    BroadcastTaskUpdated(task);
                }
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

        public async Task<bool> DeleteTask(int userId, int taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.Information("Deleting task {TaskId} for user: {UserId}", taskId, userId);
                var result = await _taskService.DeleteTaskAsync(userId, taskId);
                if (result)
                {
                    // Broadcast to all except sender
                    BroadcastTaskDeleted(taskId, userId);
                }
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in DeleteTask for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> LockTask(int userId, int taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.Information("Locking task {TaskId} for user: {UserId}", taskId, userId);
                var result = await _taskService.LockTaskAsync(userId, taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in LockTask for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UnlockTask(int userId, int taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.Information("Unlocking task {TaskId} for user: {UserId}", taskId, userId);
                var result = await _taskService.UnlockTaskAsync(userId, taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error(ex, "Exception in UnlockTask for user: {UserId}", userId);
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

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        #region Event Handlers for Real-time Broadcasting

        private void OnTaskAdded(TaskDTO task)
        {
            try
            {
                // Broadcast only to the user who owns the task
                var groupName = $"User_{task.UserId}";
                BroadcastTaskAdded(task);
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
                BroadcastTaskUpdated(task);
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
                var userId = GetTaskOwnerFromService(taskId);
                if (userId > 0)
                {
                    BroadcastTaskDeleted(taskId, userId);
                    _logger.Information("Broadcasted TaskDeleted to group: {GroupName}", $"User_{userId}");
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
                var userId = GetTaskOwnerFromService(taskId);
                if (userId > 0)
                {
                    BroadcastTaskLocked(taskId, userId);
                    _logger.Information("Broadcasted TaskLocked to group: {GroupName}", $"User_{userId}");
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
                var userId = GetTaskOwnerFromService(taskId);
                if (userId > 0)
                {
                    BroadcastTaskUnlocked(taskId, userId);
                    _logger.Information("Broadcasted TaskUnlocked to group: {GroupName}", $"User_{userId}");
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
                var userId = GetCurrentUserId();
                var task = _taskService.GetTaskByIdAsync(userId, taskId).GetAwaiter().GetResult();
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
                lock (GroupLock)
                {
                    if (!GroupMembers.ContainsKey(groupName))
                        GroupMembers[groupName] = new HashSet<string>();
                    GroupMembers[groupName].Add(Context.ConnectionId);
                }
                _logger.Information("User {UserId} connected and added to group: {GroupName}. Active connections: {ActiveConnections}", 
                    userId, groupName, _activeConnections);
            }
            
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _activeConnections = Math.Max(0, _activeConnections - 1);
            var userId = GetCurrentUserId();
            
            // Remove from all groups
            lock (GroupLock)
            {
                foreach (var group in GroupMembers.Keys)
                {
                    GroupMembers[group].Remove(Context.ConnectionId);
                }
            }
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
                lock (GroupLock)
                {
                    if (!GroupMembers.ContainsKey(groupName))
                        GroupMembers[groupName] = new HashSet<string>();
                    GroupMembers[groupName].Add(Context.ConnectionId);
                }
                _logger.Information("User {UserId} reconnected and re-added to group: {GroupName}", userId, groupName);
            }
            
            return base.OnReconnected();
        }

        #endregion

        public static int GetActiveConnections() => _activeConnections;

        // Unified broadcast method to handle all broadcast types
        private void BroadcastToUserGroup<T>(string methodName, T data, int userId)
        {
            var groupName = $"User_{userId}";
            try
            {
                // Log active connections and group membership
                _logger.Information("Active connections: {ActiveConnections}", _activeConnections);
                HashSet<string> members;
                lock (GroupLock)
                {
                    GroupMembers.TryGetValue(groupName, out members);
                }
                _logger.Information("Group {GroupName} members: {Members}", groupName, members != null ? string.Join(",", members) : "<none>");
                var clientGroup = Clients.Group(groupName);
                // Always add SenderConnectionId to the payload
                var payload = new { Data = data, SenderConnectionId = Context.ConnectionId };
                dynamic proxy = clientGroup;
                if (proxy is System.Threading.Tasks.Task)
                {
                    _logger.Warning("No clients in group {GroupName} to broadcast {MethodName}", groupName, methodName);
                }
                else
                {
                    _logger.Information("Broadcasting {MethodName} to group: {GroupName} (clients present)", methodName, groupName);
                    switch (methodName)
                    {
                        case SignalRGlobals.TaskAdded:
                            proxy.TaskAdded(payload);
                            break;
                        case SignalRGlobals.TaskUpdated:
                            proxy.TaskUpdated(payload);
                            break;
                        case SignalRGlobals.TaskDeleted:
                            proxy.TaskDeleted(payload);
                            break;
                        case SignalRGlobals.TaskLocked:
                            proxy.TaskLocked(payload);
                            break;
                        case SignalRGlobals.TaskUnlocked:
                            proxy.TaskUnlocked(payload);
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown SignalR method: {methodName}");
                    }
                }
                _logger.Information("Broadcasted {MethodName} to group: {GroupName}", methodName, groupName);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Broadcast might have failed due to no clients in group: {GroupName}", groupName);
            }
        }

        private void BroadcastTaskAdded(TaskDTO task)
        {
            BroadcastToUserGroup(SignalRGlobals.TaskAdded, task, task.UserId);
        }
        
        private void BroadcastTaskUpdated(TaskDTO task)
        {
            BroadcastToUserGroup(SignalRGlobals.TaskUpdated, task, task.UserId);
        }
        
        private void BroadcastTaskDeleted(int taskId, int userId)
        {
            BroadcastToUserGroup(SignalRGlobals.TaskDeleted, taskId, userId);
        }
        
        private void BroadcastTaskLocked(int taskId, int userId)
        {
            BroadcastToUserGroup(SignalRGlobals.TaskLocked, taskId, userId);
        }
        
        private void BroadcastTaskUnlocked(int taskId, int userId)
        {
            BroadcastToUserGroup(SignalRGlobals.TaskUnlocked, taskId, userId);
        }
    }
} 