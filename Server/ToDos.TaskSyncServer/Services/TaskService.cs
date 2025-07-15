using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using ToDos.DotNet.Common;
using ToDos.Entities;
using ToDos.Repository;
using ToDos.Server.Common.Interfaces;
using ToDos.DotNet.Caching;

namespace ToDos.TaskSyncServer.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        // Use int for userId in cache
        private readonly ICacheService<int, TaskDTO> _taskCache = new MemoryCacheService<int, TaskDTO>("TaskCache", Log.Logger);
        private const string TaskCacheKey = "tasks";

        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<int> TaskDeleted;
        public event Action<int> TaskLocked;
        public event Action<int> TaskUnlocked;

        public TaskService(ITaskRepository taskRepository, IMapper mapper, ILogger logger)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId)
        {
            try
            {
                _logger.Information("Getting tasks for user: {UserId}", userId);
                // Try cache first
                try
                {
                    var cached = _taskCache.Get(userId, TaskCacheKey);
                    if (cached != null)
                    {
                        var cachedList = cached.ToList();
                        if (cachedList.Count > 0)
                        {
                            _logger.Information("Returning cached tasks for user: {UserId}", userId);
                            return cachedList;
                        }
                    }
                }
                catch (Exception cacheEx)
                {
                    _logger.Warning(cacheEx, "Cache get failed for user: {UserId}", userId);
                }
                var tasks = await _taskRepository.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<IEnumerable<TaskDTO>>(tasks).ToList();
                try
                {
                    _taskCache.Set(userId, TaskCacheKey, dtos);
                }
                catch (Exception cacheEx)
                {
                    _logger.Warning(cacheEx, "Cache set failed for user: {UserId}", userId);
                }
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting tasks for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<TaskDTO> GetTaskByIdAsync(int userId, int taskId)
        {
            try
            {
                _logger.Information("Getting task by ID: {TaskId} for user: {UserId}", taskId, userId);
                var task = await _taskRepository.GetByIdAsync(userId, taskId);
                return _mapper.Map<TaskDTO>(task);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting task by ID: {TaskId} for user: {UserId}", taskId, userId);
                throw;
            }
        }

        public async Task<TaskDTO> AddTaskAsync(TaskDTO taskDto)
        {
            try
            {
                _logger.Information("Adding task for user: {UserId}", taskDto.UserId);
                
                var taskEntity = _mapper.Map<TaskEntity>(taskDto);
                await _taskRepository.AddAsync(taskEntity);
                
                var addedTask = _mapper.Map<TaskDTO>(taskEntity);
                
                // Raise event for real-time updates
                TaskAdded?.Invoke(addedTask);
                
                _logger.Information("Task added successfully: {TaskId}", addedTask.Id);
                // Invalidate cache for this user
                try
                {
                    _taskCache.Invalidate(taskDto.UserId, TaskCacheKey);
                }
                catch (Exception cacheEx)
                {
                    _logger.Warning(cacheEx, "Cache invalidate failed for user: {UserId}", taskDto.UserId);
                }
                return addedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding task for user: {UserId}", taskDto.UserId);
                throw;
            }
        }

        public async Task<bool> UpdateTaskAsync(TaskDTO taskDto)
        {
            try
            {
                _logger.Information("Updating task: {TaskId} for user: {UserId}", taskDto.Id, taskDto.UserId);
                
                // Verify task ownership
                var existingTask = await _taskRepository.GetByIdAsync(taskDto.UserId, taskDto.Id);
                if (existingTask == null)
                {
                    _logger.Warning("Task not found for update: {TaskId}", taskDto.Id);
                    throw new InvalidOperationException($"Task with ID {taskDto.Id} not found");
                }
                
                if (existingTask.UserId != taskDto.UserId)
                {
                    _logger.Warning("Unauthorized task update attempt. Task owner: {TaskOwner}, Requesting user: {RequestingUser}", 
                        existingTask.UserId, taskDto.UserId);
                    throw new UnauthorizedAccessException("You can only update your own tasks");
                }
                
                var taskEntity = _mapper.Map<TaskEntity>(taskDto);
                await _taskRepository.UpdateAsync(taskEntity);
                
                var updatedTask = _mapper.Map<TaskDTO>(taskEntity);
                
                // Raise event for real-time updates
                TaskUpdated?.Invoke(updatedTask);
                
                _logger.Information("Task updated successfully: {TaskId}", updatedTask.Id);
                // Invalidate cache for this user
                try
                {
                    _taskCache.Invalidate(taskDto.UserId, TaskCacheKey);
                }
                catch (Exception cacheEx)
                {
                    _logger.Warning(cacheEx, "Cache invalidate failed for user: {UserId}", taskDto.UserId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating task: {TaskId}", taskDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int userId, int taskId)
        {
            try
            {
                _logger.Information("Deleting task: {TaskId} for user: {UserId}", taskId, userId);
                
                var success = await _taskRepository.DeleteAsync(userId, taskId);
                
                if (success)
                {
                    // Raise event for real-time updates
                    TaskDeleted?.Invoke(taskId);
                    _logger.Information("Task deleted successfully: {TaskId}", taskId);
                    // Invalidate cache for this user
                    try
                    {
                        _taskCache.Invalidate(userId, TaskCacheKey);
                    }
                    catch (Exception cacheEx)
                    {
                        _logger.Warning(cacheEx, "Cache invalidate failed for user: {UserId}", userId);
                    }
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

        public async Task<bool> LockTaskAsync(int userId, int taskId)
        {
            try
            {
                _logger.Information("Locking task: {TaskId} for user: {UserId}", taskId, userId);
                
                var success = await _taskRepository.LockTaskAsync(userId, taskId);
                
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

        public async Task<bool> UnlockTaskAsync(int userId, int taskId)
        {
            try
            {
                _logger.Information("Unlocking task: {TaskId} for user: {UserId}", taskId, userId);
                
                var success = await _taskRepository.UnlockTaskAsync(userId, taskId);
                
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