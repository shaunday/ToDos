using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Todos.Ui.Models;
using static global::ToDos.DotNet.Common.GlobalTypes;


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
    }
}
