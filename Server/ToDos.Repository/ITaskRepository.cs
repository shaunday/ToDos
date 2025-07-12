using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.Entities;

namespace ToDos.Repository
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskEntity>> GetAllAsync();
        Task<TaskEntity> GetByIdAsync(Guid id);
        Task AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SetCompletionAsync(Guid taskId, bool isCompleted);

        // Locking methods
        Task<bool> LockTaskAsync(Guid id);
        Task<bool> UnlockTaskAsync(Guid id);
        Task<bool> IsTaskLockedAsync(Guid id);
    }
} 