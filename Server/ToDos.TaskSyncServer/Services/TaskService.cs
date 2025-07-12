using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using ToDos.DotNet.Common;
using ToDos.Repository;
using ToDos.Server.Common.Entities;
using ToDos.Server.Common.Interfaces;

namespace ToDos.TaskSyncServer.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<Guid> TaskDeleted;
        public event Action<Guid> TaskLocked;
        public event Action<Guid> TaskUnlocked;

        public TaskService(ITaskRepository taskRepository, IMapper mapper, ILogger logger)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            try
            {
                _logger.Information("Getting all tasks");
                var tasks = await _taskRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<TaskDTO>>(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all tasks");
                throw;
            }
        }

        public async Task<TaskDTO> AddTaskAsync(TaskDTO taskDto)
        {
            try
            {
                // Input validation
                if (taskDto == null)
                    throw new ArgumentNullException(nameof(taskDto));
                
                if (string.IsNullOrWhiteSpace(taskDto.Title))
                    throw new ArgumentException("Task title cannot be empty", nameof(taskDto));
                
                _logger.Information("Adding new task: {TaskTitle}", taskDto.Title);
                
                var taskEntity = _mapper.Map<TaskEntity>(taskDto);
                taskEntity.Id = Guid.NewGuid();
                
                await _taskRepository.AddAsync(taskEntity);
                
                var result = _mapper.Map<TaskDTO>(taskEntity);
                
                // Raise event for real-time updates
                TaskAdded?.Invoke(result);
                
                _logger.Information("Task added successfully: {TaskId}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding task: {TaskTitle}", taskDto?.Title);
                throw;
            }
        }

        public async Task<TaskDTO> UpdateTaskAsync(TaskDTO taskDto)
        {
            try
            {
                // Input validation
                if (taskDto == null)
                    throw new ArgumentNullException(nameof(taskDto));
                
                if (taskDto.Id == Guid.Empty)
                    throw new ArgumentException("Task ID cannot be empty", nameof(taskDto));
                
                if (string.IsNullOrWhiteSpace(taskDto.Title))
                    throw new ArgumentException("Task title cannot be empty", nameof(taskDto));
                
                _logger.Information("Updating task: {TaskId}", taskDto.Id);
                
                var taskEntity = _mapper.Map<TaskEntity>(taskDto);
                await _taskRepository.UpdateAsync(taskEntity);
                
                var result = _mapper.Map<TaskDTO>(taskEntity);
                
                // Raise event for real-time updates
                TaskUpdated?.Invoke(result);
                
                _logger.Information("Task updated successfully: {TaskId}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating task: {TaskId}", taskDto?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(Guid taskId)
        {
            try
            {
                _logger.Information("Deleting task: {TaskId}", taskId);
                
                var success = await _taskRepository.DeleteAsync(taskId);
                
                if (success)
                {
                    // Raise event for real-time updates
                    TaskDeleted?.Invoke(taskId);
                    _logger.Information("Task deleted successfully: {TaskId}", taskId);
                }
                else
                {
                    _logger.Warning("Task not found for deletion: {TaskId}", taskId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting task: {TaskId}", taskId);
                throw;
            }
        }

        public async Task<bool> SetTaskCompletionAsync(Guid taskId, bool isCompleted)
        {
            try
            {
                _logger.Information("Setting task completion: {TaskId}, Completed: {IsCompleted}", taskId, isCompleted);
                
                var success = await _taskRepository.SetCompletionAsync(taskId, isCompleted);
                
                if (success)
                {
                    var updatedTask = await _taskRepository.GetByIdAsync(taskId);
                    var taskDto = _mapper.Map<TaskDTO>(updatedTask);
                    
                    // Raise event for real-time updates
                    TaskUpdated?.Invoke(taskDto);
                    
                    _logger.Information("Task completion updated successfully: {TaskId}", taskId);
                }
                else
                {
                    _logger.Warning("Task not found for completion update: {TaskId}", taskId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting task completion: {TaskId}", taskId);
                throw;
            }
        }

        public async Task<bool> LockTaskAsync(Guid taskId)
        {
            try
            {
                _logger.Information("Locking task: {TaskId}", taskId);
                
                var success = await _taskRepository.LockTaskAsync(taskId);
                
                if (success)
                {
                    // Raise event for real-time updates
                    TaskLocked?.Invoke(taskId);
                    _logger.Information("Task locked successfully: {TaskId}", taskId);
                }
                else
                {
                    _logger.Warning("Task not found or already locked: {TaskId}", taskId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error locking task: {TaskId}", taskId);
                throw;
            }
        }

        public async Task<bool> UnlockTaskAsync(Guid taskId)
        {
            try
            {
                _logger.Information("Unlocking task: {TaskId}", taskId);
                
                var success = await _taskRepository.UnlockTaskAsync(taskId);
                
                if (success)
                {
                    // Raise event for real-time updates
                    TaskUnlocked?.Invoke(taskId);
                    _logger.Information("Task unlocked successfully: {TaskId}", taskId);
                }
                else
                {
                    _logger.Warning("Task not found or not locked: {TaskId}", taskId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unlocking task: {TaskId}", taskId);
                throw;
            }
        }
    }
} 