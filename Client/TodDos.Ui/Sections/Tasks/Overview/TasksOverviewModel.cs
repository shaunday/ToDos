using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace Todos.Ui.Sections.Tasks
{
    public partial class TasksOverviewModel : ObservableObject
    {
        [ObservableProperty] private int total;
        [ObservableProperty] private int locked;
        [ObservableProperty] private int high;
        [ObservableProperty] private int medium;
        [ObservableProperty] private int low;

        public void Refresh(IEnumerable<Todos.Ui.Models.TaskModel> tasks, Todos.Ui.Models.TaskModel alwaysInclude = null)
        {
            var list = tasks.ToList();
            if (alwaysInclude != null && !list.Contains(alwaysInclude))
                list.Add(alwaysInclude);
            Total = list.Count;
            Locked = list.Count(IsLockedTask);
            High = list.Count(IsHighPriority);
            Medium = list.Count(IsMediumPriority);
            Low = list.Count(IsLowPriority);
        }

        private static bool IsLockedTask(Todos.Ui.Models.TaskModel t) => t.IsLocked;
        private static bool IsHighPriority(Todos.Ui.Models.TaskModel t) => t.Priority == ToDos.DotNet.Common.TaskPriority.High;
        private static bool IsMediumPriority(Todos.Ui.Models.TaskModel t) => t.Priority == ToDos.DotNet.Common.TaskPriority.Medium;
        private static bool IsLowPriority(Todos.Ui.Models.TaskModel t) => t.Priority == ToDos.DotNet.Common.TaskPriority.Low;
    }
} 