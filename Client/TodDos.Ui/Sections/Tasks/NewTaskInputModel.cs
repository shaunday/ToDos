using CommunityToolkit.Mvvm.ComponentModel;
using System;
using ToDos.DotNet.Common;

namespace Todos.Ui.Sections.Tasks
{
    public partial class NewTaskInputModel : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private TaskPriority priority = TaskPriority.Medium;

        [ObservableProperty]
        private DateTime? dueDate;

        public NewTaskInputModel Clone()
        {
            var clone = new NewTaskInputModel();
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(NewTaskInputModel other)
        {
            Title = other.Title;
            Description = other.Description;
            Priority = other.Priority;
            DueDate = other.DueDate;
        }
    }
} 