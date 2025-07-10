using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using ToDos.Ui.Models;
using ToDos.Ui.Services.Navigation;

namespace ToDos.Ui.ViewModels
{
    public partial class TasksViewModel : ViewModelBase
    {
        private readonly ITaskSyncClient _taskService;

        public TasksViewModel(ITaskSyncClient taskService, IMapper mapper, INavigationService navigation) : base(mapper, navigation)
        {
            _taskService = taskService;
            LoadTasksAsync();
        }

        [ObservableProperty]
        private ObservableCollection<TaskModel> tasks = new ObservableCollection<TaskModel>();

        [ObservableProperty]
        private TaskModel? selectedTask;

        // Load tasks asynchronously
        private async void LoadTasksAsync()
        {
            var allTaskDtos = await _taskService.GetAllTasksAsync();
            var allTaskModels = allTaskDtos.Select(dto => _mapper.Map<TaskModel>(dto));
            tasks = new ObservableCollection<TaskModel>(allTaskModels);
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            var newTaskModel = new TaskModel { Title = "New Task" };
            var newTaskDto = _mapper.Map<TaskDTO>(newTaskModel);
            var addedTaskDto = await _taskService.AddTaskAsync(newTaskDto);
            var addedTaskModel = _mapper.Map<TaskModel>(addedTaskDto);

            tasks.Add(addedTaskModel);
            selectedTask = addedTaskModel;
        }

        [RelayCommand]
        private async Task DeleteTaskAsync()
        {
            if (selectedTask is null) return;

            var taskId = selectedTask.Id;
            var deleted = await _taskService.DeleteTaskAsync(taskId);
            if (deleted)
            {
                tasks.Remove(selectedTask);
                selectedTask = null;
            }
        }

        [RelayCommand]
        private async Task MarkCompleteAsync(bool isCompleted)
        {
            if (selectedTask is null) return;

            selectedTask.IsCompleted = isCompleted;

            var updatedDto = _mapper.Map<TaskDTO>(selectedTask);
            await _taskService.UpdateTaskAsync(updatedDto);

            // Raise property changed for Tasks or SelectedTask if needed
            OnPropertyChanged(nameof(Tasks));
            OnPropertyChanged(nameof(SelectedTask));
        }
    }
}
