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
        private async Task EditTaskAsync(TaskModel task)
        {
            if (task == null || task.IsEditing) return; // Only one edit at a time
            var locked = await _taskService.LockTaskAsync(task.Id);
            if (locked)
            {
                foreach (var t in Tasks) t.IsEditing = false;
                task.IsEditing = true;
            }
            // else: show error message (optional)
        }

        [RelayCommand]
        private async Task SaveTaskAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing) return;
            var updatedDto = _mapper.Map<TaskDTO>(task);
            await _taskService.UpdateTaskAsync(updatedDto);
            await _taskService.UnlockTaskAsync(task.Id);
            task.IsEditing = false;
        }

        [RelayCommand]
        private async Task CancelEditAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing) return;
            await _taskService.UnlockTaskAsync(task.Id);
            // Optionally reload the task from backend to discard changes
            task.IsEditing = false;
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            var newTaskModel = new TaskModel { Title = "New Task" };
            var newTaskDto = _mapper.Map<TaskDTO>(newTaskModel);
            var addedTaskDto = await _taskService.AddTaskAsync(newTaskDto);
            var addedTaskModel = _mapper.Map<TaskModel>(addedTaskDto);

            Tasks.Add(addedTaskModel);
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskModel task)
        {
            var deleted = await _taskService.DeleteTaskAsync(task.Id);
            if (deleted)
            {
                Tasks.Remove(task);
                if (task.IsEditing) task.IsEditing = false;
            }
        }

        //[RelayCommand]
        //private async Task MarkCompleteAsync(TaskModel task, bool isCompleted)
        //{
        //    task.IsCompleted = isCompleted;
        //    var updatedDto = _mapper.Map<TaskDTO>(task);
        //    await _taskService.UpdateTaskAsync(updatedDto);
        //    OnPropertyChanged(nameof(Tasks));
        //}
    }
}
