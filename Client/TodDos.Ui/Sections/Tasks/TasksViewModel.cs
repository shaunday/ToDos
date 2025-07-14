using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Todos.Client.Common.Interfaces;
using ToDos.DotNet.Common;
using Todos.Ui.Sections.Tasks;
using Todos.Ui.Services.Navigation;
using Todos.Ui.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using TodDos.Ui.Global.ViewModels;
using Serilog;
using Todos.Client.UserService.Interfaces;
using ToDos.MockAuthService;

namespace Todos.Ui.ViewModels
{
    public partial class TasksViewModel : ViewModelBase, IInitializable, ICleanable
    {
        #region Fields
        private readonly IAuthService _authService;
        public SortableCollectionViewModel<TaskModel> SortableTaskCollection { get; } // Expose as public property

        [ObservableProperty]
        private TaskModel editingTask;

        [ObservableProperty]
        private bool isAddingNewTask = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        // Backup for editing a task
        [ObservableProperty]
        private TaskModel editingTaskBackup;

        // Buffer for adding a new task
        [ObservableProperty]
        private NewTaskInputModel newTaskBuffer;

        [ObservableProperty]
        private TaskFilter filter = new TaskFilter();

        public TasksOverviewModel Overview { get; } = new TasksOverviewModel();
        public TasksOverviewModel FilteredOverview { get; } = new TasksOverviewModel();
        #endregion

        #region Properties
        // Use Items and CollectionView from sortable member
        public ObservableCollection<TaskModel> Tasks => SortableTaskCollection.Items;
        public ICollectionView FilteredTasksView => CollectionViewSource.GetDefaultView(Tasks);
        public string SortProperty => SortableTaskCollection.SortProperty;
        public System.ComponentModel.ListSortDirection SortDirection => SortableTaskCollection.SortDirection;
        #endregion

        #region Constructors and Lifecycle
        public TasksViewModel(ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation, ILogger logger, IAuthService authService)
            : base(taskSyncClient, mapper, navigation, logger)
        {
            _authService = authService;
            SortableTaskCollection = new SortableCollectionViewModel<TaskModel>();
            Overview.Refresh(Tasks);
            // Rebind CollectionView to new Tasks collection
            FilteredTasksView.Refresh();
            UpdateFilteredTasks();
        }

        public override void Init()
        {
            _taskSyncClient!.TaskAdded += HandleTaskAdded;
            _taskSyncClient.TaskUpdated += HandleTaskUpdated;
            _taskSyncClient.TaskDeleted += HandleTaskDeleted;
            Tasks.CollectionChanged += Tasks_CollectionChanged;
            Filter.PropertyChanged += Filter_PropertyChanged;
            
            // Load existing tasks for the current user
            LoadTasksAsync();
        }

        public override void Cleanup()
        {
            _taskSyncClient!.TaskAdded -= HandleTaskAdded;
            _taskSyncClient.TaskUpdated -= HandleTaskUpdated;
            _taskSyncClient.TaskDeleted -= HandleTaskDeleted;
            Tasks.CollectionChanged -= Tasks_CollectionChanged;
            Filter.PropertyChanged -= Filter_PropertyChanged;
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ShowAddTaskForm()
        {
            IsAddingNewTask = true;
            NewTaskBuffer = new NewTaskInputModel();
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void CancelAddTask()
        {
            IsAddingNewTask = false;
            NewTaskBuffer = null;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            if (NewTaskBuffer == null || string.IsNullOrWhiteSpace(NewTaskBuffer.Title))
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
                    Title = NewTaskBuffer.Title.Trim(),
                    Description = NewTaskBuffer.Description?.Trim() ?? string.Empty,
                    Priority = NewTaskBuffer.Priority,
                    DueDate = NewTaskBuffer.DueDate,
                    IsCompleted = false,
                    IsLocked = false,
                    IsEditing = false,
                    Tags = NewTaskBuffer.Tags?.Trim() ?? string.Empty
                };

                var newTaskDto = _mapper!.Map<TaskDTO>(newTaskModel);
                var addedTaskDto = await _taskSyncClient!.AddTaskAsync(newTaskDto);
                // Do NOT add to Tasks here; rely on HandleTaskAdded event
                // var addedTaskModel = _mapper.Map<TaskModel>(addedTaskDto);
                // Tasks.Add(addedTaskModel);
                // Reset form
                IsAddingNewTask = false;
                NewTaskBuffer = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add task");
                ErrorMessage = $"Failed to add task.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task EditTaskAsync(TaskModel task)
        {
            if (task == null || EditingTaskBackup != null) return; // Only one edit at a time

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
                _logger.Error(ex, "Failed to edit task");
                ErrorMessage = $"Failed to edit task.";
            }
        }

        [RelayCommand]
        private async Task SaveTaskAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing || EditingTaskBackup == null) return;

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                ErrorMessage = "Task title is required.";
                return;
            }

            try
            {
                ErrorMessage = string.Empty;
                // No need to copy from backup, just save the current task
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
                await _taskSyncClient.UnlockTaskAsync(task.Id);
                ChangeTaskEditMode(task, false);
                Overview.Refresh(Tasks);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save task");
                ErrorMessage = $"Failed to save task.";
            }
        }

        [RelayCommand]
        private async Task CancelEditAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing || EditingTaskBackup == null) return;
            try
            {
                ErrorMessage = string.Empty;
                await _taskSyncClient!.UnlockTaskAsync(task.Id);
                // Restore original values from backup
                task.CopyFrom(EditingTaskBackup);
                ChangeTaskEditMode(task, false);
                Overview.Refresh(Tasks);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to cancel edit");
                ErrorMessage = $"Failed to cancel edit.";
            }
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
                    Overview.Refresh(Tasks);
                }
                else
                {
                    ErrorMessage = "Failed to delete task.";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete task");
                ErrorMessage = $"Failed to delete task.";
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
                Overview.Refresh(Tasks);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update task completion");
                // Revert the change on error
                task.IsCompleted = !task.IsCompleted;
                ErrorMessage = $"Failed to update task completion.";
            }
        }


        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void UpdateFilteredTasks()
        {
            FilteredTasksView.Refresh();
            // Update filtered overview
            var filtered = Filter.Apply(Tasks);
            FilteredOverview.Refresh(filtered);
        }

        private bool FilterPredicate(object obj)
        {
            var t = obj as TaskModel;
            if (t == null) return false;
            return Filter.Apply(new[] { t }).Any();
        }

        private void ChangeTaskEditMode(TaskModel task, bool isEditing)
        {
            task.IsEditing = isEditing;
            EditingTask = isEditing ? task : null;
            EditingTaskBackup = isEditing ? task.Clone() : null;
        }

        private async void LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                // Get current user ID from the user service or application context
                var currentUserId = GetCurrentUserId();
                
                if (currentUserId > 0)
                {
                    var userTaskDtos = await _taskSyncClient!.GetUserTasksAsync(currentUserId);
                    
                    var userTaskModels = userTaskDtos.Select(dto => _mapper!.Map<TaskModel>(dto));
                    // Instead of replacing the collection, update it in place:
                    SortableTaskCollection.Items.Clear();
                    foreach (var item in userTaskModels)
                        SortableTaskCollection.Items.Add(item);
                    Overview.Refresh(Tasks);
                    // Rebind CollectionView to new Tasks collection
                    FilteredTasksView.Refresh();
                    UpdateFilteredTasks();
                }
                else
                {
                    _logger.Warning("No valid user ID found for loading tasks");
                    ErrorMessage = "User not authenticated. Please log in.";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load tasks");
                ErrorMessage = $"Failed to load tasks.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private int GetCurrentUserId()
        {
            try
            {
                // Get the JWT token from the task sync client
                var jwtToken = _taskSyncClient?.GetJwtToken();
                
                if (string.IsNullOrEmpty(jwtToken))
                {
                    _logger.Warning("No JWT token available for getting user ID");
                    return 0;
                }

                // Use AuthService to extract user ID from token
                var userId = _authService.GetUserIdFromTokenWithoutValidation(jwtToken);
                return userId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting user ID from JWT token");
                return 0;
            }
        }
        #endregion

        #region Event Handlers
        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Overview.Refresh(Tasks);
            UpdateFilteredTasks();
        }

        private void HandleTaskAdded(TaskDTO taskDto)
        {
            var taskModel = _mapper!.Map<TaskModel>(taskDto);
            Tasks.Add(taskModel);
            Overview.Refresh(Tasks);
            UpdateFilteredTasks();
        }

        private void HandleTaskUpdated(TaskDTO taskDto)
        {
            var taskModel = _mapper!.Map<TaskModel>(taskDto);
            var existingTask = Tasks.FirstOrDefault(t => t.Id == taskDto.Id);
            if (existingTask != null)
            {
                var index = Tasks.IndexOf(existingTask);
                Tasks[index] = taskModel;
                Overview.Refresh(Tasks);
                UpdateFilteredTasks();
            }
        }

        private void HandleTaskDeleted(int taskId)
        {
            var taskToRemove = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (taskToRemove != null)
            {
                Tasks.Remove(taskToRemove);
                Overview.Refresh(Tasks);
                UpdateFilteredTasks();
            }
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateFilteredTasks();
        }
        #endregion
    }
}

