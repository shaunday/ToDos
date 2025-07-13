using System;
using System.Collections.Generic;
using ToDos.DotNet.Common;

namespace Todos.Server.MockTaskService
{
    public static class MockTaskData
    {
        public static List<TaskDTO> GetSampleTasks()
        {
            return new List<TaskDTO>
            {
                new TaskDTO 
                { 
                    Id = 1, 
                    UserId = 1,
                    Title = "Complete project documentation", 
                    Description = "Write comprehensive documentation for the new feature including API specs and user guides", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(3),
                    Tags = new List<TagDTO> { new TagDTO { Name = "documentation" }, new TagDTO { Name = "api" }, new TagDTO { Name = "feature" } }
                },
                new TaskDTO 
                { 
                    Id = 2, 
                    UserId = 1,
                    Title = "Review pull requests", 
                    Description = "Review 5 pending pull requests for the backend team", 
                    IsCompleted = true,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(-1),
                    Tags = new List<TagDTO> { new TagDTO { Name = "review" }, new TagDTO { Name = "backend" }, new TagDTO { Name = "pr" } }
                },
                new TaskDTO 
                { 
                    Id = 3, 
                    UserId = 1,
                    Title = "Fix critical bug in login system", 
                    Description = "Users are experiencing authentication failures on mobile devices", 
                    IsCompleted = false, 
                    IsLocked = true,
                    Priority = "High",
                    DueDate = DateTime.Now.AddHours(6),
                    Tags = new List<TagDTO> { new TagDTO { Name = "bug" }, new TagDTO { Name = "login" }, new TagDTO { Name = "critical" } }
                },
                new TaskDTO 
                { 
                    Id = 4, 
                    UserId = 1,
                    Title = "Update dependencies", 
                    Description = "Update all NuGet packages to latest stable versions and test for breaking changes", 
                    IsCompleted = false,
                    Priority = "Low",
                    DueDate = DateTime.Now.AddDays(7),
                    Tags = new List<TagDTO> { new TagDTO { Name = "dependencies" }, new TagDTO { Name = "nuget" }, new TagDTO { Name = "update" } }
                },
                new TaskDTO 
                { 
                    Id = 5, 
                    UserId = 1,
                    Title = "Prepare presentation for stakeholders", 
                    Description = "Create slides for quarterly review meeting with key metrics and progress updates", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(2),
                    Tags = new List<TagDTO> { new TagDTO { Name = "presentation" }, new TagDTO { Name = "stakeholders" }, new TagDTO { Name = "meeting" } }
                },
                new TaskDTO 
                { 
                    Id = 6, 
                    UserId = 2,
                    Title = "Code review for new feature", 
                    Description = "Review the implementation of the new search functionality and provide feedback", 
                    IsCompleted = true,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(-2),
                    Tags = new List<TagDTO> { new TagDTO { Name = "review" }, new TagDTO { Name = "feature" }, new TagDTO { Name = "search" } }
                },
                new TaskDTO 
                { 
                    Id = 7, 
                    UserId = 2,
                    Title = "Setup CI/CD pipeline", 
                    Description = "Configure automated testing and deployment pipeline for the new microservice", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(5),
                    Tags = new List<TagDTO> { new TagDTO { Name = "ci" }, new TagDTO { Name = "cd" }, new TagDTO { Name = "automation" }, new TagDTO { Name = "devops" } }
                },
                new TaskDTO 
                { 
                    Id = 8, 
                    UserId = 2,
                    Title = "Database optimization", 
                    Description = "Analyze and optimize slow queries in the production database", 
                    IsCompleted = false,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(10),
                    Tags = new List<TagDTO> { new TagDTO { Name = "database" }, new TagDTO { Name = "optimization" }, new TagDTO { Name = "performance" } }
                },
                new TaskDTO 
                { 
                    Id = 9, 
                    UserId = 2,
                    Title = "Security audit", 
                    Description = "Conduct security review of authentication and authorization systems", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(1),
                    Tags = new List<TagDTO> { new TagDTO { Name = "security" }, new TagDTO { Name = "audit" }, new TagDTO { Name = "auth" } }
                },
                new TaskDTO 
                { 
                    Id = 10, 
                    UserId = 1,
                    Title = "Update user manual", 
                    Description = "Revise user documentation to reflect recent UI changes and new features", 
                    IsCompleted = false,
                    Priority = "Low",
                    DueDate = DateTime.Now.AddDays(14),
                    Tags = new List<TagDTO> { new TagDTO { Name = "documentation" }, new TagDTO { Name = "user" }, new TagDTO { Name = "manual" } }
                },
                new TaskDTO 
                { 
                    Id = 11, 
                    UserId = 2,
                    Title = "Performance testing", 
                    Description = "Run load tests on the new API endpoints to ensure they meet performance requirements", 
                    IsCompleted = true,
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(-3),
                    Tags = new List<TagDTO> { new TagDTO { Name = "performance" }, new TagDTO { Name = "testing" }, new TagDTO { Name = "api" } }
                },
                new TaskDTO 
                { 
                    Id = 12, 
                    UserId = 1,
                    Title = "Bug fix: Memory leak", 
                    Description = "Investigate and fix memory leak reported in the image processing module", 
                    IsCompleted = false,
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(4),
                    Tags = new List<TagDTO> { new TagDTO { Name = "bug" }, new TagDTO { Name = "memory" }, new TagDTO { Name = "leak" } }
                }
            };
        }
    }
} 