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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // TaskEntity configuration
            modelBuilder.Entity<ToDos.Entities.TaskEntity>()
                .HasKey(t => t.Id)
                .Property(t => t.Title).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<ToDos.Entities.TaskEntity>()
                .Property(t => t.Description).HasMaxLength(100);
            // TagEntity configuration
            modelBuilder.Entity<ToDos.Entities.TagEntity>()
                .HasKey(t => t.Id)
                .Property(t => t.Name).IsRequired().HasMaxLength(100);
            // Many-to-many relationship (if needed)
            modelBuilder.Entity<ToDos.Entities.TaskEntity>()
                .HasMany(t => t.Tags)
                .WithMany(tg => tg.Tasks)
                .Map(m =>
                {
                    m.ToTable("TaskTags");
                    m.MapLeftKey("TaskId");
                    m.MapRightKey("TagId");
                });
            base.OnModelCreating(modelBuilder);
        }
    }
} 