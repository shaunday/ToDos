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

        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<Guid> TaskDeleted;
        public event Action<Guid> TaskLocked;
        public event Action<Guid> TaskUnlocked;

        public MockTaskService(ILogger logger)
        {
            _logger = logger;
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            var mockTasks = new[]
            {
                new TaskDTO
                {
                    Id = Guid.NewGuid(),
                    Title = "Complete SignalR Implementation",
                    Description = "Implement the SignalR hub for real-time task synchronization",
                    IsCompleted = false,
                    IsLocked = false,
                    Priority = TaskPriority.High.ToString(),
                    DueDate = DateTime.Now.AddDays(2),
                    Tags = new List<TagDTO> { new TagDTO { Id = Guid.NewGuid(), Name = "Development" } }
                },
                new TaskDTO
                {
                    Id = Guid.NewGuid(),
                    Title = "Write Unit Tests",
                    Description = "Create comprehensive unit tests for the task service",
                    IsCompleted = true,
                    IsLocked = false,
                    Priority = TaskPriority.Medium.ToString(),
                    DueDate = DateTime.Now.AddDays(1),
                    Tags = new List<TagDTO> { new TagDTO { Id = Guid.NewGuid(), Name = "Testing" } }
                },
                new TaskDTO
                {
                    Id = Guid.NewGuid(),
                    Title = "Update Documentation",
                    Description = "Update README with setup instructions and architecture overview",
                    IsCompleted = false,
                    IsLocked = false,
                    Priority = TaskPriority.Low.ToString(),
                    DueDate = DateTime.Now.AddDays(3),
                    Tags = new List<TagDTO> { new TagDTO { Id = Guid.NewGuid(), Name = "Documentation" } }
                }
            };

            lock (_lockObject)
            {
                _tasks.AddRange(mockTasks);
            }
        }

        public async Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            await Task.Delay(50); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Getting all tasks. Count: {TaskCount}", _tasks.Count);
                return _tasks.ToList();
            }
        }

        public async Task<TaskDTO> AddTaskAsync(TaskDTO taskDto)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Adding new task: {TaskTitle}", taskDto.Title);
                
                taskDto.Id = Guid.NewGuid();
                _tasks.Add(taskDto);
                
                // Raise event for real-time updates
                TaskAdded?.Invoke(taskDto);
                
                _logger.Information("Task added successfully: {TaskId}", taskDto.Id);
                return taskDto;
            }
        }

        public async Task<TaskDTO> UpdateTaskAsync(TaskDTO taskDto)
        {
            await Task.Delay(100); // Simulate async operation
            
            lock (_lockObject)
            {
                _logger.Information("Updating task: {TaskId}", taskDto.Id);
                
                var existingTask = _tasks.FirstOrDefault(t => t.Id == taskDto.Id);
                if (existingTask == null)
                {
                    _logger.Warning("Task not found for update: {TaskId}", taskDto.Id);
                    throw new InvalidOperationException($"Task with ID {taskDto.Id} not found");
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
                
                _logger.Information("Task updated successfully: {TaskId}", existingTask.Id);
                return existingTask;
            }
        }

        public async Task<bool> DeleteTaskAsync(Guid taskId)
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

        public async Task<bool> SetTaskCompletionAsync(Guid taskId, bool isCompleted)
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

        public async Task<bool> LockTaskAsync(Guid taskId)
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

        public async Task<bool> UnlockTaskAsync(Guid taskId)
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