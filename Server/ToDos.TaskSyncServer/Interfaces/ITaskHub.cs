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
        Task<bool> DeleteTask(int userId, int taskId);
        Task<bool> LockTask(int userId, int taskId);
        Task<bool> UnlockTask(int userId, int taskId);
        Task<IEnumerable<TaskDTO>> GetUserTasks(int userId);
    }
} 