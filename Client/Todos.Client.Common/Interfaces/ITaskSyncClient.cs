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

        // JWT Token management
        void SetJwtToken(string jwt);
        string GetJwtToken();

        // Connects to the server (real-time sync or API handshake)
        Task ConnectAsync();

        // Disconnects from server
        Task DisconnectAsync();

        // CRUD operations
        Task<TaskDTO> AddTaskAsync(TaskDTO task);
        Task<TaskDTO> UpdateTaskAsync(TaskDTO task);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<bool> LockTaskAsync(int taskId);
        Task<bool> UnlockTaskAsync(int taskId);

        // Get tasks by user ID for initial loading
        Task<IEnumerable<TaskDTO>> GetUserTasksAsync(int userId);

        // Events raised when tasks change in real-time
        event Action<TaskDTO> TaskAdded;
        event Action<TaskDTO> TaskUpdated;
        event Action<int> TaskDeleted;
        event Action<int> TaskLocked;
        event Action<int> TaskUnlocked;
    }
}
