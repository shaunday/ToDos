using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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
using MaterialDesignThemes.Wpf;

namespace Todos.Ui.ViewModels
{
    public partial class TasksViewModel : ViewModelBase, IInitializable, ICleanable
    {
        #region Fields
        private readonly IAuthService _authService;

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

        public ICollectionView FilteredTasksView { get; private set; }

        public TasksOverviewModel Overview { get; } = new TasksOverviewModel();
        public TasksOverviewModel FilteredOverview { get; } = new TasksOverviewModel();
        public SnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue();
        #endregion

        #region Properties
        public ObservableCollection<TaskModel> Tasks { get; set; } = new ObservableCollection<TaskModel>();
        #endregion

        #region Constructors and Lifecycle
        public TasksViewModel(ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation, ILogger logger, IAuthService authService)
            : base(taskSyncClient, mapper, navigation, logger)
        {
            _authService = authService;
            Overview.Refresh(Tasks);
            FilteredTasksView = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasksView.Filter = FilterPredicate;
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

            IsLoading = true;
            ErrorMessage = string.Empty;
            await RunWithErrorHandlingAsync(async () =>
            {
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
                IsAddingNewTask = false;
                NewTaskBuffer = null;
            }, "Failed to add task.", SnackbarMessageQueue);
            IsLoading = false;
        }

        [RelayCommand]
        private async Task EditTaskAsync(TaskModel task)
        {
            if (task == null || EditingTaskBackup != null) return; // Only one edit at a time

            await RunWithErrorHandlingAsync(async () =>
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
                    SnackbarMessageQueue.Enqueue(ErrorMessage);
                }
            }, "Failed to edit task.", SnackbarMessageQueue);
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

            await RunWithErrorHandlingAsync(async () =>
            {
                ErrorMessage = string.Empty;
                // No need to copy from backup, just save the current task
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
                await _taskSyncClient.UnlockTaskAsync(task.Id);
                ChangeTaskEditMode(task, false);
                Overview.Refresh(Tasks);
            }, "Failed to save task.", SnackbarMessageQueue);
        }

        [RelayCommand]
        private async Task CancelEditAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing || EditingTaskBackup == null) return;
            await RunWithErrorHandlingAsync(async () =>
            {
                ErrorMessage = string.Empty;
                await _taskSyncClient!.UnlockTaskAsync(task.Id);
                // Restore original values from backup
                task.CopyFrom(EditingTaskBackup);
                ChangeTaskEditMode(task, false);
                Overview.Refresh(Tasks);
            }, "Failed to cancel edit.", SnackbarMessageQueue);
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskModel task)
        {
            if (task == null) return;

            await RunWithErrorHandlingAsync(async () =>
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
                    SnackbarMessageQueue.Enqueue(ErrorMessage);
                }
            }, "Failed to delete task.", SnackbarMessageQueue);
        }

        [RelayCommand]
        private async Task ToggleTaskCompletionAsync(TaskModel task)
        {
            if (task == null) return;

            await RunWithErrorHandlingAsync(async () =>
            {
                ErrorMessage = string.Empty;
                task.IsCompleted = !task.IsCompleted;
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
                Overview.Refresh(Tasks);
            }, "Failed to update task completion.", SnackbarMessageQueue);
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
            IsLoading = true;
            ErrorMessage = string.Empty;
            await RunWithErrorHandlingAsync(async () =>
            {
                // Get current user ID from the user service or application context
                var currentUserId = GetCurrentUserId();

                if (currentUserId > 0)
                {
                    var userTaskDtos = await _taskSyncClient!.GetUserTasksAsync(currentUserId);

                    var userTaskModels = userTaskDtos.Select(dto => _mapper!.Map<TaskModel>(dto));
                    Tasks = new ObservableCollection<TaskModel>(userTaskModels);
                    Overview.Refresh(Tasks);
                    // Rebind CollectionView to new Tasks collection
                    FilteredTasksView = CollectionViewSource.GetDefaultView(Tasks);
                    FilteredTasksView.Filter = FilterPredicate;
                    UpdateFilteredTasks();
                }
                else
                {
                    _logger.Warning("No valid user ID found for loading tasks");
                    ErrorMessage = "User not authenticated. Please log in.";
                    SnackbarMessageQueue.Enqueue(ErrorMessage);
                }
            }, "Failed to load tasks.", SnackbarMessageQueue);
            IsLoading = false;
        }

        private int GetCurrentUserId()
        {
            // Get the JWT token from the task sync client
            var jwtToken = _taskSyncClient?.GetJwtToken();

            if (string.IsNullOrEmpty(jwtToken))
            {
                _logger.Warning("No JWT token available for getting user ID");
                throw new InvalidOperationException("No JWT token available for getting user ID");
            }

            // Use AuthService to extract user ID from token
            return _authService.GetUserIdFromTokenWithoutValidation(jwtToken);
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

