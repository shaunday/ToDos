using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDos.DotNet.Common;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Client.Common.Interfaces
{
    public interface ITaskSyncClient
    {
        // Connection state
        ConnectionStatus ConnectionStatus { get; }
        event Action<ConnectionStatus> ConnectionStatusChanged;

        // Connects to the server (real-time sync or API handshake)
        Task ConnectAsync();

        // Disconnects from server
        Task DisconnectAsync();

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
