using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Todos.Ui.Models;
using ToDos.DotNet.Common;

namespace Todos.Ui.Sections.Tasks
{
    public partial class TaskFilter : ObservableObject
    {
        [ObservableProperty] private string selectedPriority = "All";
        [ObservableProperty] private string tagFilter = string.Empty;
        [ObservableProperty] private string completedStatus = "All";

        public IEnumerable<TaskModel> Apply(IEnumerable<TaskModel> tasks)
        {
            var filtered = tasks;
            if (!string.IsNullOrWhiteSpace(SelectedPriority) && SelectedPriority != "All")
                filtered = filtered.Where(t => t.Priority.ToString() == SelectedPriority);
            if (!string.IsNullOrWhiteSpace(TagFilter))
                filtered = filtered.Where(t => (t.Tags ?? string.Empty).ToLowerInvariant().Contains(TagFilter.Trim().ToLowerInvariant()));
            if (!string.IsNullOrWhiteSpace(CompletedStatus) && CompletedStatus != "All")
            {
                if (CompletedStatus == "Completed")
                    filtered = filtered.Where(t => t.IsCompleted);
                else if (CompletedStatus == "Not Completed")
                    filtered = filtered.Where(t => !t.IsCompleted);
            }
            return filtered;
        }
    }
} 