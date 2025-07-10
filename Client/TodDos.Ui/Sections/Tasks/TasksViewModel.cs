using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using ToDos.Ui.Services.Navigation;
using ToDos.Ui.ViewModels;

namespace ToDos.Ui.Views.Tasks
{
    public partial class TasksViewModel : ViewModelBase
    {
        private readonly ITaskService _taskService;
        private readonly INavigationService _navigation;

        public TasksViewModel(ITaskService taskService, INavigationService navigation)
        {
            _taskService = taskService;
            _navigation = navigation;

            LoadTasks();
        }

        [ObservableProperty]
        private ObservableCollection<TaskModel> tasks = new();

        [ObservableProperty]
        private TaskModel? selectedTask;

        // Load tasks from DB/service
        private void LoadTasks()
        {
            var allTasks = _taskService.GetAllTasks(); // sync or async; adjust accordingly
            Tasks = new ObservableCollection<TaskModel>(allTasks);
        }

        [RelayCommand]
        private void AddTask()
        {
            var newTask = new TaskModel { Title = "New Task" };
            _taskService.AddTask(newTask);
            Tasks.Add(newTask);
            SelectedTask = newTask;
        }

        [RelayCommand]
        private void DeleteTask()
        {
            if (SelectedTask is null) return;

            _taskService.DeleteTask(SelectedTask);
            Tasks.Remove(SelectedTask);
            SelectedTask = null;
        }

        [RelayCommand]
        private void MarkComplete()
        {
            if (SelectedTask is null) return;

            SelectedTask.IsCompleted = true;
            _taskService.UpdateTask(SelectedTask);

            // Notify UI that task updated
            OnPropertyChanged(nameof(Tasks));
        }

        // Example of navigation or other commands can be added here

        // Optionally add EditTask command, etc.
    }
}
