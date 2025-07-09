using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.Entities;

namespace ToDos.Repository
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskEntity>> GetAllAsync();
        Task<TaskEntity> GetByIdAsync(int id);
        Task AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(int id);

        // Locking methods
        Task<bool> LockTaskAsync(int id);
        Task<bool> UnlockTaskAsync(int id);
        Task<bool> IsTaskLockedAsync(int id);
    }
} 