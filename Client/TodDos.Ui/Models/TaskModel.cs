using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Todos.Ui.Models;
using ToDos.DotNet.Common;
using System.Linq;

namespace Todos.Ui.Models
{
    public partial class TaskModel : ObservableObject
    {
        [ObservableProperty]
        private Guid id;

        [ObservableProperty]
        private Guid userId;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private bool isLocked;

        [ObservableProperty]
        private bool isEditing = false;

        [ObservableProperty]
        private TaskPriority priority;

        [ObservableProperty]
        private DateTime? dueDate;

        [ObservableProperty]
        private ObservableCollection<TagModel> tags = new ObservableCollection<TagModel>();

        public TaskModel Clone()
        {
            var clone = new TaskModel();
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(TaskModel other)
        {
            Id = other.Id;
            UserId = other.UserId;
            Title = other.Title;
            Description = other.Description;
            IsCompleted = other.IsCompleted;
            IsLocked = other.IsLocked;
            IsEditing = other.IsEditing;
            Priority = other.Priority;
            DueDate = other.DueDate;
            Tags = new ObservableCollection<TagModel>(other.Tags.Select(tag => new TagModel { Name = tag.Name }));
        }
    }
}
