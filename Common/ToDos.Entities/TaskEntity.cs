using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDos.Entities
{
    public enum TaskPriority { Low, Medium, High }

    public class TaskEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
