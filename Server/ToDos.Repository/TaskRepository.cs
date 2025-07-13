using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ToDos.Entities;

namespace ToDos.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskDbContext _context;
        private readonly ILogger _logger;

        public TaskRepository(TaskDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskEntity>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Tasks
                    .Where(t => t.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in GetByUserIdAsync for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<TaskEntity> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Tasks.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in GetByIdAsync");
                throw;
            }
        }

        public async Task AddAsync(TaskEntity task)
        {
            try
            {
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            try
            {
                var existing = await _context.Tasks.FindAsync(task.Id);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(task);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in UpdateAsync");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int taskId)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return false;

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in DeleteAsync");
                throw;
            }
        }

        public async Task<bool> SetCompletionAsync(int taskId, bool isCompleted)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return false;

                task.IsCompleted = isCompleted;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in SetCompletionAsync");
                throw;
            }
        }

        public async Task<bool> LockTaskAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null || task.IsLocked)
                    return false;

                task.IsLocked = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LockTaskAsync (Guid)");
                throw;
            }
        }

        public async Task<bool> UnlockTaskAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null || !task.IsLocked)
                    return false;

                task.IsLocked = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in UnlockTaskAsync (Guid)");
                throw;
            }
        }

        public async Task<bool> IsTaskLockedAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                return task?.IsLocked ?? false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in IsTaskLockedAsync (Guid)");
                throw;
            }
        }
    }
}