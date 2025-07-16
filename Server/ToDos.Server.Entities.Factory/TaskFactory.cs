using System;
using System.Collections.Generic;
using ToDos.DotNet.Common;

namespace ToDos.Server.Entities.Factory
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; } // Add priority property
        public List<TagDTO> Tags { get; set; } // Add tags property
    }

    public static class TaskFactory
    {
        /// <summary>
        /// Generates a list of tasks for each user ID provided.
        /// </summary>
        public static List<TaskEntity> GenerateTasksForUsers(IEnumerable<int> userIds, int tasksPerUser = 5)
        {
            var result = new List<TaskEntity>();
            var rand = new Random();
            var priorities = Enum.GetValues(typeof(TaskPriority));
            // Define possible tags
            var possibleTags = new List<TagDTO>
            {
                new TagDTO { Id = Guid.NewGuid(), Name = "Work" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Home" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Urgent" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Low Priority" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Personal" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Shopping" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Fitness" },
                new TagDTO { Id = Guid.NewGuid(), Name = "Finance" }
            };
            foreach (var userId in userIds)
            {
                for (int i = 0; i < tasksPerUser; i++)
                {
                    // Pick 1-3 random tags for each task
                    int tagCount = rand.Next(1, 4);
                    var tags = new List<TagDTO>();
                    var usedIndexes = new HashSet<int>();
                    while (tags.Count < tagCount)
                    {
                        int idx = rand.Next(possibleTags.Count);
                        if (!usedIndexes.Contains(idx))
                        {
                            usedIndexes.Add(idx);
                            tags.Add(possibleTags[idx]);
                        }
                    }
                    result.Add(new TaskEntity
                    {
                        UserId = userId,
                        Title = $"Task {i + 1} for User {userId}",
                        Description = $"Auto-generated task {i + 1} for user {userId}",
                        IsCompleted = rand.Next(2) == 0,
                        Priority = (TaskPriority)priorities.GetValue(rand.Next(priorities.Length)), // Assign random priority
                        Tags = tags
                    });
                }
            }
            return result;
        }
    }
} 