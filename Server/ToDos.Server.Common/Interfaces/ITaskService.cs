using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;

namespace ToDos.Server.Common.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDTO>> GetAllTasksAsync();
        Task<TaskDTO> AddTaskAsync(TaskDTO task);
        Task<TaskDTO> UpdateTaskAsync(TaskDTO task);
        Task<bool> DeleteTaskAsync(Guid taskId);
        Task<bool> SetTaskCompletionAsync(Guid taskId, bool isCompleted);
        Task<bool> LockTaskAsync(Guid taskId);
        Task<bool> UnlockTaskAsync(Guid taskId);
        
        // Events for real-time updates
        event Action<TaskDTO> TaskAdded;
        event Action<TaskDTO> TaskUpdated;
        event Action<Guid> TaskDeleted;
        event Action<Guid> TaskLocked;
        event Action<Guid> TaskUnlocked;
    }
} 