using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;

namespace ToDos.Server.Common.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId);
        Task<TaskDTO> GetTaskByIdAsync(int userId, int taskId);
        Task<TaskDTO> AddTaskAsync(TaskDTO task);
        Task<TaskDTO> UpdateTaskAsync(TaskDTO task);
        Task<bool> DeleteTaskAsync(int userId, int taskId);
        Task<bool> LockTaskAsync(int userId, int taskId);
        Task<bool> UnlockTaskAsync(int userId, int taskId);
        
        // Events for real-time updates
        event Action<TaskDTO> TaskAdded;
        event Action<TaskDTO> TaskUpdated;
        event Action<int> TaskDeleted;
        event Action<int> TaskLocked;
        event Action<int> TaskUnlocked;
    }
} 