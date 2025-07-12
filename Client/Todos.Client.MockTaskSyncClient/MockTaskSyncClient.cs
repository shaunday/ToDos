using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Client.MockTaskSyncClient
{
    public class MockTaskSyncClient : ITaskSyncClient
    {
        private ConnectionStatus _connectionStatus = ConnectionStatus.Connected;
        
        public ConnectionStatus ConnectionStatus 
        { 
            get => _connectionStatus;
            private set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    ConnectionStatusChanged?.Invoke(_connectionStatus);
                }
            }
        }
        
        public event Action<ConnectionStatus> ConnectionStatusChanged;
        public event Action<TaskDTO> TaskAdded;
        public event Action<TaskDTO> TaskUpdated;
        public event Action<Guid> TaskDeleted;
        public event Action<Guid> TaskLocked;
        public event Action<Guid> TaskUnlocked;

        public Task ConnectAsync()
        {
            ConnectionStatus = ConnectionStatus.Connecting;
            // Simulate connection delay
            Task.Delay(500).ContinueWith(_ => ConnectionStatus = ConnectionStatus.Connected);
            return Task.CompletedTask;
        }
        
        public Task DisconnectAsync()
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            var tasks = new List<TaskDTO>
            {
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Complete project documentation", 
                    Description = "Write comprehensive documentation for the new feature including API specs and user guides", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(3)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Review pull requests", 
                    Description = "Review 5 pending pull requests for the backend team", 
                    IsCompleted = true,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(-1)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Fix critical bug in login system", 
                    Description = "Users are experiencing authentication failures on mobile devices", 
                    IsCompleted = false, 
                    IsLocked = true,
                    Priority = "High",
                    DueDate = DateTime.Now.AddHours(6)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Update dependencies", 
                    Description = "Update all NuGet packages to latest stable versions and test for breaking changes", 
                    IsCompleted = false,
                    Priority = "Low",
                    DueDate = DateTime.Now.AddDays(7)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Prepare presentation for stakeholders", 
                    Description = "Create slides for quarterly review meeting with key metrics and progress updates", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(2)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Code review for new feature", 
                    Description = "Review the implementation of the new search functionality and provide feedback", 
                    IsCompleted = true,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(-2)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Setup CI/CD pipeline", 
                    Description = "Configure automated testing and deployment pipeline for the new microservice", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(5)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Database optimization", 
                    Description = "Analyze and optimize slow queries in the production database", 
                    IsCompleted = false,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(10)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Security audit", 
                    Description = "Conduct security review of authentication and authorization systems", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(1)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Update user manual", 
                    Description = "Revise user documentation to reflect recent UI changes and new features", 
                    IsCompleted = false,
                    Priority = "Low",
                    DueDate = DateTime.Now.AddDays(14)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Performance testing", 
                    Description = "Run load tests on the new API endpoints to ensure they meet performance requirements", 
                    IsCompleted = true,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(-3)
                },
                new TaskDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Bug fix: Memory leak", 
                    Description = "Investigate and fix memory leak reported in the image processing module", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(4)
                }
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