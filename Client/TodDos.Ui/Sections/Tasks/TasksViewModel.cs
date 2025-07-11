using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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
            ConnectAndLoadTasksAsync();
        }

        [ObservableProperty]
        private ObservableCollection<TaskModel> tasks = new ObservableCollection<TaskModel>();

        [ObservableProperty]
        private TaskModel? selectedTask;

        private async void ConnectAndLoadTasksAsync()
        {
            try
            {
                await _taskService.ConnectAsync();
                var allTaskDtos = await _taskService.GetAllTasksAsync();
                var allTaskModels = allTaskDtos.Select(dto => _mapper.Map<TaskModel>(dto));
                Tasks = new ObservableCollection<TaskModel>(allTaskModels);
            }
            catch (Exception ex)
            {
                // Optionally, expose an error message property for the UI
            }
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            var newTaskModel = new TaskModel { Title = "New Task" };
            var newTaskDto = _mapper.Map<TaskDTO>(newTaskModel);
            var addedTaskDto = await _taskService.AddTaskAsync(newTaskDto);
            var addedTaskModel = _mapper.Map<TaskModel>(addedTaskDto);

            Tasks.Add(addedTaskModel);
            SelectedTask = addedTaskModel;
        }

        [RelayCommand]
        private async Task DeleteTaskAsync()
        {
            if (SelectedTask is null) return;

            var taskId = SelectedTask.Id;
            var deleted = await _taskService.DeleteTaskAsync(taskId);
            if (deleted)
            {
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
        }

        [RelayCommand]
        private async Task MarkCompleteAsync(bool isCompleted)
        {
            if (SelectedTask is null) return;

            SelectedTask.IsCompleted = isCompleted;

            var updatedDto = _mapper.Map<TaskDTO>(SelectedTask);
            await _taskService.UpdateTaskAsync(updatedDto);

            // Raise property changed for Tasks or SelectedTask if needed
            OnPropertyChanged(nameof(Tasks));
            OnPropertyChanged(nameof(SelectedTask));
        }
    }
}
