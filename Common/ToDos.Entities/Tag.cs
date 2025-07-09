using System.Collections.Generic;

namespace ToDos.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TaskEntity> Tasks { get; set; }
    }
} 