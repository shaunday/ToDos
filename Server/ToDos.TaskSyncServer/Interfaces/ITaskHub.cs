using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;

namespace ToDos.TaskSyncServer.Interfaces
{
    public interface ITaskHub
    {
        Task<IEnumerable<TaskDTO>> GetAllTasks();
        Task<TaskDTO> AddTask(TaskDTO task);
        Task<TaskDTO> UpdateTask(TaskDTO task);
        Task<bool> DeleteTask(Guid taskId);
        Task<bool> SetTaskCompletion(Guid taskId, bool isCompleted);
        Task<bool> LockTask(Guid taskId);
        Task<bool> UnlockTask(Guid taskId);
    }
} 