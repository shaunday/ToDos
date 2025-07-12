using Microsoft.AspNet.SignalR;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ToDos.DotNet.Common;
using ToDos.Server.Common.Interfaces;

namespace ToDos.TaskSyncServer.Hubs
{
    public class TaskHub : Hub, ITaskHub
    {
        private readonly ITaskService _taskService;
        private static int _activeConnections = 0;

        public TaskHub(ITaskService taskService)
        {
            _taskService = taskService;
            
            // Subscribe to service events for real-time broadcasting
            _taskService.TaskAdded += OnTaskAdded;
            _taskService.TaskUpdated += OnTaskUpdated;
            _taskService.TaskDeleted += OnTaskDeleted;
            _taskService.TaskLocked += OnTaskLocked;
            _taskService.TaskUnlocked += OnTaskUnlocked;
        }

        public async Task<System.Collections.Generic.IEnumerable<TaskDTO>> GetAllTasks()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.GetAllTasksAsync();
                stopwatch.Stop();
                // Log performance metrics
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Log error with performance data
                throw;
            }
        }

        public async Task<TaskDTO> AddTask(TaskDTO task)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.AddTaskAsync(task);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw;
            }
        }

        public async Task<TaskDTO> UpdateTask(TaskDTO task)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.UpdateTaskAsync(task);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw;
            }
        }

        public async Task<bool> DeleteTask(Guid taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.DeleteTaskAsync(taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw;
            }
        }

        public async Task<bool> SetTaskCompletion(Guid taskId, bool isCompleted)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.SetTaskCompletionAsync(taskId, isCompleted);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw;
            }
        }

        public async Task<bool> LockTask(Guid taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.LockTaskAsync(taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw;
            }
        }

        public async Task<bool> UnlockTask(Guid taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _taskService.UnlockTaskAsync(taskId);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw;
            }
        }

        #region Event Handlers for Real-time Broadcasting

        private void OnTaskAdded(TaskDTO task)
        {
            Clients.All.TaskAdded(task);
        }

        private void OnTaskUpdated(TaskDTO task)
        {
            Clients.All.TaskUpdated(task);
        }

        private void OnTaskDeleted(Guid taskId)
        {
            Clients.All.TaskDeleted(taskId);
        }

        private void OnTaskLocked(Guid taskId)
        {
            Clients.All.TaskLocked(taskId);
        }

        private void OnTaskUnlocked(Guid taskId)
        {
            Clients.All.TaskUnlocked(taskId);
        }

        #endregion

        #region SignalR Lifecycle Methods

        public override Task OnConnected()
        {
            _activeConnections++;
            // Log connection with active count
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _activeConnections = Math.Max(0, _activeConnections - 1);
            // Log disconnection with active count
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            // Log reconnection
            return base.OnReconnected();
        }

        #endregion

        public static int GetActiveConnections() => _activeConnections;
    }
} 