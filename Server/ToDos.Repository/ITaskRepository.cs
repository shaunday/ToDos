using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.Entities;

namespace ToDos.Repository
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskEntity>> GetByUserIdAsync(int userId);
        Task<TaskEntity> GetByIdAsync(int userId, int id);
        Task AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task<bool> DeleteAsync(int userId, int id);

        // Locking methods
        Task<bool> LockTaskAsync(int userId, int id);
        Task<bool> UnlockTaskAsync(int userId, int id);
    }
} 