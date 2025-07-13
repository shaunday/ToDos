using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ToDos.DotNet.Common;
using ToDos.Server.Common.Interfaces;
using static ToDos.DotNet.Common.Globals;

namespace Todos.Server.MockTaskService
{
    public class MockTaskService : ITaskService
    {
        private readonly ILogger _logger;
        private readonly List<TaskDTO> _tasks = new List<TaskDTO>();
        private readonly object _lockObject = new object();
        private int _nextTaskId = 1;

        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<int> TaskDeleted;
        public event Action<int> TaskLocked;
        public event Action<int> TaskUnlocked;

        public MockTaskService(ILogger logger)
        {
            _logger = logger;
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            lock (_lockObject)
            {
                _tasks.Clear();
                _nextTaskId = 13; // Start after the sample data IDs
                
                // Add sample tasks from MockTaskData
                var sampleTasks = MockTaskData.GetSampleTasks();
                _tasks.AddRange(sampleTasks);
                
                _logger.Information("Mock data initialized with {TaskCount} sample tasks", _tasks.Count);
            }
        }

        public async Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Getting tasks for user: {UserId}", userId);
                var userTasks = _tasks.Where(t => t.UserId == userId).ToList();
                _logger.Information("Found {TaskCount} tasks for user {UserId}", userTasks.Count, userId);
                return userTasks;
            }
        }

        public async Task<TaskDTO> GetTaskByIdAsync(int taskId)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Getting task by ID: {TaskId}", taskId);
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.Warning("Task not found: {TaskId}", taskId);
                }
                return task;
            }
        }

        public async Task<TaskDTO> AddTaskAsync(TaskDTO taskDto)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Adding task for user: {UserId}", taskDto.UserId);
                
                if (string.IsNullOrWhiteSpace(taskDto.Title))
                {
                    _logger.Warning("Task title is empty for user: {UserId}", taskDto.UserId);
                    throw new ArgumentException("Task title cannot be empty");
                }
                
                taskDto.Id = _nextTaskId++;
                _tasks.Add(taskDto);
                
                // Raise event for real-time updates
                TaskAdded?.Invoke(taskDto);
                
                _logger.Information("Task added successfully: {TaskId} for user: {UserId}", taskDto.Id, taskDto.UserId);
                return taskDto;
            }
        }

        public async Task<TaskDTO> UpdateTaskAsync(TaskDTO taskDto)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Updating task: {TaskId} for user: {UserId}", taskDto.Id, taskDto.UserId);
                
                var existingTask = _tasks.FirstOrDefault(t => t.Id == taskDto.Id);
                if (existingTask == null)
                {
                    _logger.Warning("Task not found for update: {TaskId}", taskDto.Id);
                    throw new InvalidOperationException($"Task with ID {taskDto.Id} not found");
                }
                
                // Verify task ownership
                if (existingTask.UserId != taskDto.UserId)
                {
                    _logger.Warning("Unauthorized task update attempt. Task owner: {TaskOwner}, Requesting user: {RequestingUser}", 
                        existingTask.UserId, taskDto.UserId);
                    throw new UnauthorizedAccessException("You can only update your own tasks");
                }
                
                // Update properties
                existingTask.Title = taskDto.Title;
                existingTask.Description = taskDto.Description;
                existingTask.IsCompleted = taskDto.IsCompleted;
                existingTask.Priority = taskDto.Priority;
                existingTask.DueDate = taskDto.DueDate;
                existingTask.Tags = taskDto.Tags;
                
                // Raise event for real-time updates
                TaskUpdated?.Invoke(existingTask);
                
                _logger.Information("Task updated successfully: {TaskId} for user: {UserId}", existingTask.Id, existingTask.UserId);
                return existingTask;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Deleting task: {TaskId}", taskId);
                
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.Warning("Task not found for deletion: {TaskId}", taskId);
                    return false;
                }
                
                _tasks.Remove(task);
                
                // Raise event for real-time updates
                TaskDeleted?.Invoke(taskId);
                
                _logger.Information("Task deleted successfully: {TaskId}", taskId);
                return true;
            }
        }

        public async Task<bool> SetTaskCompletionAsync(int taskId, bool isCompleted)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Setting task completion: {TaskId}, Completed: {IsCompleted}", taskId, isCompleted);
                
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.Warning("Task not found for completion update: {TaskId}", taskId);
                    return false;
                }
                
                task.IsCompleted = isCompleted;
                
                // Raise event for real-time updates
                TaskUpdated?.Invoke(task);
                
                _logger.Information("Task completion updated successfully: {TaskId}", taskId);
                return true;
            }
        }

        public async Task<bool> LockTaskAsync(int taskId)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Locking task: {TaskId}", taskId);
                
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.Warning("Task not found for locking: {TaskId}", taskId);
                    return false;
                }
                
                if (task.IsLocked)
                {
                    _logger.Warning("Task already locked: {TaskId}", taskId);
                    return false;
                }
                
                task.IsLocked = true;
                
                // Raise event for real-time updates
                TaskLocked?.Invoke(taskId);
                
                _logger.Information("Task locked successfully: {TaskId}", taskId);
                return true;
            }
        }

        public async Task<bool> UnlockTaskAsync(int taskId)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Unlocking task: {TaskId}", taskId);
                
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.Warning("Task not found for unlocking: {TaskId}", taskId);
                    return false;
                }
                
                if (!task.IsLocked)
                {
                    _logger.Warning("Task not locked: {TaskId}", taskId);
                    return false;
                }
                
                task.IsLocked = false;
                
                // Raise event for real-time updates
                TaskUnlocked?.Invoke(taskId);
                
                _logger.Information("Task unlocked successfully: {TaskId}", taskId);
                return true;
            }
        }
    }
} 