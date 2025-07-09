using System.Data.Entity;
using ToDos.Entities;

namespace ToDos.Repository
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext() : base("name=TaskDbContext") { }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
} 