using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.Entities;

namespace ToDos.Repository
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskEntity>> GetByUserIdAsync(int userId);
        Task<TaskEntity> GetByIdAsync(int id);
        Task AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task<bool> DeleteAsync(int id);
        Task<bool> SetCompletionAsync(int taskId, bool isCompleted);

        // Locking methods
        Task<bool> LockTaskAsync(int id);
        Task<bool> UnlockTaskAsync(int id);
        Task<bool> IsTaskLockedAsync(int id);
    }
} 