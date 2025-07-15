using System;
using System.Collections.Generic;

namespace ToDos.DotNet.Common
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }
        public int UserId { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public List<TagDTO> Tags { get; set; }
    }
} 