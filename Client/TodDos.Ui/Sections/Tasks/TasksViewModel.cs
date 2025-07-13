using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using Todos.Ui.Models;
using Todos.Ui.Services.Navigation;

namespace Todos.Ui.ViewModels
{
    public partial class TasksViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<TaskModel> tasks = new ObservableCollection<TaskModel>();

        [ObservableProperty]
        private TaskModel? editingTask;

        public TasksViewModel(ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation)
            : base(taskSyncClient, mapper, navigation)
        {
            LoadTasksAsync();
        }

        private async void LoadTasksAsync()
        {
            try
            {
                var allTaskDtos = await _taskSyncClient.GetAllTasksAsync();
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
            if (task == null || EditingTask != null) return; // Only one edit at a time

            var locked = await _taskSyncClient.LockTaskAsync(task.Id);
            if (locked)
            {
                ChangeTaskEditMode(task, true);
            }
            // else: show error message (optional)
        }

        [RelayCommand]
        private async Task SaveTaskAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing) return;
            var updatedDto = _mapper.Map<TaskDTO>(task);
            await _taskSyncClient.UpdateTaskAsync(updatedDto);
            await _taskSyncClient.UnlockTaskAsync(task.Id);
            ChangeTaskEditMode(task, false);
        }

        [RelayCommand]
        private async Task CancelEditAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing) return;
            await _taskSyncClient.UnlockTaskAsync(task.Id);
            //todo if failed what now?
            //todo reload the task
            ChangeTaskEditMode(task, false);
        }

        private void ChangeTaskEditMode(TaskModel task, bool isEditing)
        {
            task.IsEditing = isEditing;
            EditingTask = isEditing ? task : null;
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            var newTaskModel = new TaskModel { Title = "New Task" };
            var newTaskDto = _mapper.Map<TaskDTO>(newTaskModel);
            var addedTaskDto = await _taskSyncClient.AddTaskAsync(newTaskDto);
            var addedTaskModel = _mapper.Map<TaskModel>(addedTaskDto);

            Tasks.Add(addedTaskModel);
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskModel task)
        {
            var deleted = await _taskSyncClient.DeleteTaskAsync(task.Id);
            if (deleted)
            {
                Tasks.Remove(task);
            }
        }

        //[RelayCommand]
        //private async Task MarkCompleteAsync(TaskModel task, bool isCompleted)
        //{
        //    task.IsCompleted = isCompleted;
        //    var updatedDto = _mapper.Map<TaskDTO>(task);
        //    await _taskSyncClient.UpdateTaskAsync(updatedDto);
        //    OnPropertyChanged(nameof(Tasks));
        //}
    }
}
