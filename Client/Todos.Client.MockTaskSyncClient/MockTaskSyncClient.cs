using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;

namespace Todos.Client.MockTaskSyncClient
{
    public class MockTaskSyncClient : ITaskSyncClient
    {
        public bool IsConnected => true;
        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<Guid> TaskDeleted;
        public event Action<Guid> TaskLocked;
        public event Action<Guid> TaskUnlocked;

        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            var tasks = new List<TaskDTO>
            {
                new TaskDTO { Id = Guid.NewGuid(), Title = "Mock Task 1", Description = "Test task 1", IsCompleted = false },
                new TaskDTO { Id = Guid.NewGuid(), Title = "Mock Task 2", Description = "Test task 2", IsCompleted = true },
                new TaskDTO { Id = Guid.NewGuid(), Title = "Mock Task 3", Description = "Test task 3", IsCompleted = false, IsLocked = true }
            };
            return Task.FromResult<IEnumerable<TaskDTO>>(tasks);
        }

        public Task<TaskDTO> AddTaskAsync(TaskDTO task)
        {
            task.Id = Guid.NewGuid();
            TaskAdded?.Invoke(task);
            return Task.FromResult(task);
        }

        public Task<TaskDTO> UpdateTaskAsync(TaskDTO task)
        {
            TaskUpdated?.Invoke(task);
            return Task.FromResult(task);
        }

        public Task<bool> DeleteTaskAsync(Guid taskId)
        {
            TaskDeleted?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<bool> SetTaskCompletionAsync(Guid taskId, bool isCompleted)
        {
            // For mock, just return true
            return Task.FromResult(true);
        }

        public Task<bool> LockTaskAsync(Guid taskId)
        {
            TaskLocked?.Invoke(taskId);
            return Task.FromResult(true);
        }

        public Task<bool> UnlockTaskAsync(Guid taskId)
        {
            TaskUnlocked?.Invoke(taskId);
            return Task.FromResult(true);
        }
    }
} 