using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;

namespace Todos.Client.Common.Interfaces
{
   
    public interface ITaskSyncClient
    {
        // Connects to the server (real-time sync or API handshake)
        Task ConnectAsync();

        // Disconnects from server
        Task DisconnectAsync();

        // Indicates whether the client is connected to the backend
        bool IsConnected { get; }

        // CRUD operations
        Task<IEnumerable<TaskDTO>> GetAllTasksAsync();
        Task<TaskDTO> AddTaskAsync(TaskDTO task);
        Task<TaskDTO> UpdateTaskAsync(TaskDTO task);
        Task<bool> DeleteTaskAsync(Guid taskId);
        Task<bool> SetTaskCompletionAsync(Guid taskId, bool isCompleted);

        // Events raised when tasks change in real-time
        event Action<TaskDTO> TaskAdded;
        event Action<TaskDTO> TaskUpdated;
        event Action<Guid> TaskDeleted;

        // Optional: For task locking
        Task<bool> LockTaskAsync(Guid taskId);
        Task<bool> UnlockTaskAsync(Guid taskId);
        event Action<Guid> TaskLocked;
        event Action<Guid> TaskUnlocked;
    }

}
