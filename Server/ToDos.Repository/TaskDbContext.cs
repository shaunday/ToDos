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
            /*
             * Database Normalization (1NF, 2NF, 3NF) and Extensibility
             *
             * 1NF (First Normal Form):
             *   - All attributes (columns) are atomic: each field contains only a single value, not a list or set.
             *     (e.g., Task.Title is a single string, not a comma-separated list of titles.)
             *
             * 2NF (Second Normal Form):
             *   - All non-key attributes are fully dependent on the entire primary key (PK).
             *   - Since TaskEntity and TagEntity have single-column PKs (Id), all other fields must describe only that entity.
             *     (e.g., Task.Description describes only the task, not something else.)
             *
             * 3NF (Third Normal Form):
             *   - No transitive dependencies: non-key attributes do not depend on other non-key attributes.
             *   - All non-key attributes depend only on the PK, not on other non-key fields.
             *     (e.g., Task.Title does not depend on Task.Description.)
             *
             * This schema is normalized to 3NF and is extensible:
             *   - You can add new fields or relationships without breaking normalization.
             *   - Many-to-many Task <-> Tag relationship is flexible for future needs.
             */
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