using CommunityToolkit.Mvvm.ComponentModel;
using System;
using ToDos.DotNet.Common;

namespace Todos.Ui.Models
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
    }
} 