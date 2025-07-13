using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;

namespace ToDos.TaskSyncServer.Interfaces
{
    public interface ITaskHub
    {
        Task<TaskDTO> AddTask(TaskDTO task);
        Task<TaskDTO> UpdateTask(TaskDTO task);
        Task<bool> DeleteTask(int taskId);
        Task<bool> SetTaskCompletion(int taskId, bool isCompleted);
        Task<bool> LockTask(int taskId);
        Task<bool> UnlockTask(int taskId);
        Task<IEnumerable<TaskDTO>> GetUserTasks(int userId);
    }
} 