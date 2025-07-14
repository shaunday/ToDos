using System.Data.Entity;
using ToDos.Entities;

namespace ToDos.Repository
{
    public class TaskDbContext : DbContext
    {
        // Constructor accepting a full connection string
        public TaskDbContext(string connectionString) : base(connectionString) { }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<TagEntity> Tags { get; set; }
    }
} 