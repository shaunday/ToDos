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

        static TaskRepository()
        {
            if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File("ToDos.Repository.log")
                    .CreateLogger();
            }
        }

        public TaskRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskEntity>> GetAllAsync()
        {
            try
            {
                return await _context.Tasks.ToListAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception in GetAllAsync");
                throw;
            }
        }

        public async Task<TaskEntity> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Tasks.FindAsync(id);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception in GetByIdAsync");
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
                Serilog.Log.Error(ex, "Exception in AddAsync");
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
                Serilog.Log.Error(ex, "Exception in UpdateAsync");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid taskId)
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
                Serilog.Log.Error(ex, "Exception in DeleteAsync");
                throw;
            }
        }

        public async Task<bool> SetCompletionAsync(Guid taskId, bool isCompleted)
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
                Serilog.Log.Error(ex, "Exception in SetCompletionAsync");
                throw;
            }
        }

        public async Task<bool> LockTaskAsync(Guid id)
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
                Serilog.Log.Error(ex, "Exception in LockTaskAsync (Guid)");
                throw;
            }
        }

        public async Task<bool> UnlockTaskAsync(Guid id)
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
                Serilog.Log.Error(ex, "Exception in UnlockTaskAsync (Guid)");
                throw;
            }
        }

        public async Task<bool> IsTaskLockedAsync(Guid id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                return task?.IsLocked ?? false;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception in IsTaskLockedAsync (Guid)");
                throw;
            }
        }

        // Locking logic
        public async Task<bool> LockTaskAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task != null && !task.IsLocked)
                {
                    task.IsLocked = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception in LockTaskAsync (int)");
                throw;
            }
        }

        public async Task<bool> UnlockTaskAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task != null && task.IsLocked)
                {
                    task.IsLocked = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception in UnlockTaskAsync (int)");
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
                Serilog.Log.Error(ex, "Exception in IsTaskLockedAsync (int)");
                throw;
            }
        }
    }
}