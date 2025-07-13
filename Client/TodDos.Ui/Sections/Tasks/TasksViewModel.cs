using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using Todos.Ui.Sections.Tasks;
using Todos.Ui.Services.Navigation;
using Todos.Ui.Models;

namespace Todos.Ui.ViewModels
{
    public partial class TasksViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<TaskModel> tasks = new ObservableCollection<TaskModel>();

        [ObservableProperty]
        private TaskModel? editingTask;

        [ObservableProperty]
        private NewTaskInputModel newTask = new NewTaskInputModel();

        [ObservableProperty]
        private bool isAddingNewTask = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        public TasksViewModel(ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation)
            : base(taskSyncClient, mapper, navigation)
        {
            // Subscribe to real-time task events
            _taskSyncClient!.TaskAdded += HandleTaskAdded;
            _taskSyncClient.TaskUpdated += HandleTaskUpdated;
            _taskSyncClient.TaskDeleted += HandleTaskDeleted;
            
            LoadTasksAsync();
        }

        private async void LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                var allTaskDtos = await _taskSyncClient!.GetAllTasksAsync();
                var allTaskModels = allTaskDtos.Select(dto => _mapper!.Map<TaskModel>(dto));
                Tasks = new ObservableCollection<TaskModel>(allTaskModels);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load tasks: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ShowAddTaskForm()
        {
            IsAddingNewTask = true;
            NewTask = new NewTaskInputModel();
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void CancelAddTask()
        {
            IsAddingNewTask = false;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTask.Title))
            {
                ErrorMessage = "Task title is required.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var newTaskModel = new TaskModel
                {
                    Title = NewTask.Title.Trim(),
                    Description = NewTask.Description?.Trim() ?? string.Empty,
                    Priority = NewTask.Priority,
                    DueDate = NewTask.DueDate,
                    IsCompleted = false,
                    IsLocked = false,
                    IsEditing = false,
                    Tags = new ObservableCollection<TagModel>()
                };

                var newTaskDto = _mapper!.Map<TaskDTO>(newTaskModel);
                var addedTaskDto = await _taskSyncClient!.AddTaskAsync(newTaskDto);
                var addedTaskModel = _mapper.Map<TaskModel>(addedTaskDto);

                Tasks.Add(addedTaskModel);
                
                // Reset form
                IsAddingNewTask = false;
                NewTask = new NewTaskInputModel();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add task: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task EditTaskAsync(TaskModel task)
        {
            if (task == null || EditingTask != null) return; // Only one edit at a time

            try
            {
                ErrorMessage = string.Empty;
                var locked = await _taskSyncClient!.LockTaskAsync(task.Id);
                if (locked)
                {
                    ChangeTaskEditMode(task, true);
                }
                else
                {
                    ErrorMessage = "Task is currently being edited by another user.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to edit task: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SaveTaskAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing) return;

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                ErrorMessage = "Task title is required.";
                return;
            }

            try
            {
                ErrorMessage = string.Empty;
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
                await _taskSyncClient.UnlockTaskAsync(task.Id);
                ChangeTaskEditMode(task, false);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save task: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task CancelEditAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing) return;
            
            try
            {
                ErrorMessage = string.Empty;
                await _taskSyncClient!.UnlockTaskAsync(task.Id);
                // Reload the task to get the original data
                var taskDto = await _taskSyncClient.GetAllTasksAsync();
                var originalTask = taskDto.FirstOrDefault(t => t.Id == task.Id);
                if (originalTask != null)
                {
                    var originalTaskModel = _mapper!.Map<TaskModel>(originalTask);
                    var index = Tasks.IndexOf(task);
                    if (index >= 0)
                    {
                        Tasks[index] = originalTaskModel;
                    }
                }
                ChangeTaskEditMode(task, false);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to cancel edit: {ex.Message}";
            }
        }

        private void ChangeTaskEditMode(TaskModel task, bool isEditing)
        {
            task.IsEditing = isEditing;
            EditingTask = isEditing ? task : null;
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskModel task)
        {
            if (task == null) return;

            try
            {
                ErrorMessage = string.Empty;
                var deleted = await _taskSyncClient!.DeleteTaskAsync(task.Id);
                if (deleted)
                {
                    Tasks.Remove(task);
                }
                else
                {
                    ErrorMessage = "Failed to delete task.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete task: {ex.Message}";
            }
        }

                [RelayCommand]
        private async Task ToggleTaskCompletionAsync(TaskModel task)
        {
            if (task == null) return;

            try
            {
                ErrorMessage = string.Empty;
                task.IsCompleted = !task.IsCompleted;
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
            }
            catch (Exception ex)
            {
                // Revert the change on error
                task.IsCompleted = !task.IsCompleted;
                ErrorMessage = $"Failed to update task completion: {ex.Message}";
            }
        }

        // Real-time event handlers
        private void HandleTaskAdded(TaskDTO taskDto)
        {
            var taskModel = _mapper!.Map<TaskModel>(taskDto);
            Tasks.Add(taskModel);
        }

        private void HandleTaskUpdated(TaskDTO taskDto)
        {
            var taskModel = _mapper!.Map<TaskModel>(taskDto);
            var existingTask = Tasks.FirstOrDefault(t => t.Id == taskDto.Id);
            if (existingTask != null)
            {
                var index = Tasks.IndexOf(existingTask);
                Tasks[index] = taskModel;
            }
        }

        private void HandleTaskDeleted(Guid taskId)
        {
            var taskToRemove = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (taskToRemove != null)
            {
                Tasks.Remove(taskToRemove);
            }
        }

 
    }
}
