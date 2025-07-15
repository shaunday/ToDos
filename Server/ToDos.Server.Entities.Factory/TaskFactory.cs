using System;
using System.Collections.Generic;

namespace ToDos.Server.Entities.Factory
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
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
            int id = 1;
            foreach (var userId in userIds)
            {
                for (int i = 0; i < tasksPerUser; i++)
                {
                    result.Add(new TaskEntity
                    {
                        UserId = userId,
                        Title = $"Task {i + 1} for User {userId}",
                        Description = $"Auto-generated task {i + 1} for user {userId}",
                        IsCompleted = rand.Next(2) == 0
                    });
                }
            }
            return result;
        }
    }
} 