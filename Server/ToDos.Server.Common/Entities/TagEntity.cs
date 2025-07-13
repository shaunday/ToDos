using System;
using System.Collections.Generic;
namespace ToDos.Entities
{
    public class TagEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TaskEntity> Tasks { get; set; }
    }
} 