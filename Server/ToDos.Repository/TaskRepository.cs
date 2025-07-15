using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ToDos.Entities;
using ToDos.Repository.Sharding;

namespace ToDos.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly IShardResolver _shardResolver;
        private readonly ILogger _logger;

        public TaskRepository(IShardResolver shardResolver, ILogger logger)
        {
            _shardResolver = shardResolver;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskEntity>> GetByUserIdAsync(int userId)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    return await context.Tasks
                    .Where(t => t.UserId == userId)
                        .ToListAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in GetByUserIdAsync for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<TaskEntity> GetByIdAsync(int userId, int id)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    return await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                }
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
                var connStr = _shardResolver.GetConnectionString(task.UserId);
                using (var context = new TaskDbContext(connStr))
                {
                    context.Tasks.Add(task);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
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
                var connStr = _shardResolver.GetConnectionString(task.UserId);
                using (var context = new TaskDbContext(connStr))
                {
                    var existing = await context.Tasks.FindAsync(task.Id).ConfigureAwait(false);
                if (existing != null)
                {
                        context.Entry(existing).CurrentValues.SetValues(task);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in UpdateAsync");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int userId, int taskId)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId).ConfigureAwait(false);
                if (task == null)
                    return false;

                    context.Tasks.Remove(task);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in DeleteAsync");
                throw;
            }
        }

        public async Task<bool> SetCompletionAsync(int userId, int taskId, bool isCompleted)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId).ConfigureAwait(false);
                if (task == null)
                    return false;

                task.IsCompleted = isCompleted;
                    await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in SetCompletionAsync");
                throw;
            }
        }

        public async Task<bool> LockTaskAsync(int userId, int id)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                if (task == null || task.IsLocked)
                    return false;

                task.IsLocked = true;
                    await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LockTaskAsync");
                throw;
            }
        }

        public async Task<bool> UnlockTaskAsync(int userId, int id)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                if (task == null || !task.IsLocked)
                    return false;

                task.IsLocked = false;
                    await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in UnlockTaskAsync");
                throw;
            }
        }

        public async Task<bool> IsTaskLockedAsync(int userId, int id)
        {
            try
            {
                var connStr = _shardResolver.GetConnectionString(userId);
                using (var context = new TaskDbContext(connStr))
                {
                    var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                return task?.IsLocked ?? false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in IsTaskLockedAsync");
                throw;
            }
        }
    }
}