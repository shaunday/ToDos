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
using System.ComponentModel;
using System.Windows.Data;
using TodDos.Ui.Global.ViewModels;
using Serilog;
using ToDos.MockAuthService;
using MaterialDesignThemes.Wpf;
using CommunityToolkit.Mvvm.Messaging;

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
        private string addTaskErrorMessage = string.Empty;

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
        public static UiStateModel UiState { get; set; }
        // Removed _lastSelectedTaskId field entirely
        #endregion

        #region Properties
        public ObservableCollection<TaskModel> Tasks { get; set; } = new ObservableCollection<TaskModel>();
        #endregion

        #region Constructors and Lifecycle
        public TasksViewModel(ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation, ILogger logger, IAuthService authService)
            : base(taskSyncClient, mapper, navigation, logger)
        {
            _authService = authService;

            FilteredTasksView = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasksView.Filter = FilterPredicate;
        }

        public override async void Init()
        {
            _taskSyncClient!.TaskAdded += HandleTaskAdded;
            _taskSyncClient.TaskUpdated += HandleTaskUpdated;
            _taskSyncClient.TaskDeleted += HandleTaskDeleted;
            Tasks.CollectionChanged += Tasks_CollectionChanged;
            Filter.PropertyChanged += Filter_PropertyChanged;
            
            await LoadTasksAsync();
            ApplyUiState();
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
            AddTaskErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void CancelAddTask()
        {
            IsAddingNewTask = false;
            NewTaskBuffer = null;
            AddTaskErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            if (NewTaskBuffer == null || string.IsNullOrWhiteSpace(NewTaskBuffer.Title))
            {
                AddTaskErrorMessage = "Task title is required.";
                return;
            }

            IsLoading = true;
            AddTaskErrorMessage = string.Empty;
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
                AddTaskErrorMessage = string.Empty;
                var locked = await _taskSyncClient!.LockTaskAsync(task.Id);
                if (locked)
                {
                    ChangeTaskEditMode(task, true);
                }
                else
                {
                    AddTaskErrorMessage = "Task is currently being edited by another user.";
                    SnackbarMessageQueue.Enqueue(AddTaskErrorMessage);
                }
            }, "Failed to edit task.", SnackbarMessageQueue);
        }

        [RelayCommand]
        private async Task SaveTaskAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing || EditingTaskBackup == null) return;

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                AddTaskErrorMessage = "Task title is required.";
                return;
            }

            await RunWithErrorHandlingAsync(async () =>
            {
                AddTaskErrorMessage = string.Empty;
                // No need to copy from backup, just save the current task
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
                await _taskSyncClient.UnlockTaskAsync(task.Id);
                ChangeTaskEditMode(task, false);
                UpdateFilteredTasks();
            }, "Failed to save task.", SnackbarMessageQueue, () => { ChangeTaskEditMode(task, false); return Task.CompletedTask; });
        }

        [RelayCommand]
        private async Task CancelEditAsync(TaskModel task)
        {
            if (task == null || !task.IsEditing || EditingTaskBackup == null) return;
            await RunWithErrorHandlingAsync(async () =>
            {
                AddTaskErrorMessage = string.Empty;
                await _taskSyncClient!.UnlockTaskAsync(task.Id);
                // Restore original values from backup
                task.CopyFrom(EditingTaskBackup);
                ChangeTaskEditMode(task, false);
                UpdateFilteredTasks();
            }, "Failed to cancel edit.", SnackbarMessageQueue, () => { ChangeTaskEditMode(task, false); return Task.CompletedTask; });
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskModel task)
        {
            if (task == null) return;

            await RunWithErrorHandlingAsync(async () =>
            {
                AddTaskErrorMessage = string.Empty;
                var deleted = await _taskSyncClient!.DeleteTaskAsync(task.Id);
                if (deleted)
                {
                    Tasks.Remove(task);
                    Overview.Refresh(Tasks);
                }
                else
                {
                    AddTaskErrorMessage = "Failed to delete task.";
                    SnackbarMessageQueue.Enqueue(AddTaskErrorMessage);
                }
            }, "Failed to delete task.", SnackbarMessageQueue);
        }

        [RelayCommand]
        private async Task ToggleTaskCompletionAsync(TaskModel task)
        {
            if (task == null) return;

            await RunWithErrorHandlingAsync(async () =>
            {
                AddTaskErrorMessage = string.Empty;
                task.IsCompleted = !task.IsCompleted;
                var updatedDto = _mapper!.Map<TaskDTO>(task);
                await _taskSyncClient!.UpdateTaskAsync(updatedDto);
                Overview.Refresh(Tasks);
            }, "Failed to update task completion.", SnackbarMessageQueue);
        }
        #endregion

        #region Public Methods
        public void ApplyUiState()
        {
            if (UiState != null)
            {
                if (Filter != null)
                {
                    Filter.SelectedPriority = UiState.FilterSelectedPriority ?? "All";
                    Filter.TagFilter = UiState.FilterTag ?? string.Empty;
                    Filter.CompletedStatus = UiState.FilterCompletedStatus ?? "All";
                }
                if (UiState.LastSelectedTaskId.HasValue)
                {
                    var task = Tasks.FirstOrDefault(t => t.Id == UiState.LastSelectedTaskId.Value);
                    if (task != null)
                    {
                        EditingTask = task;
                    }
                }
            }
        }
        public void UpdateUiStateFromCurrent()
        {
            if (UiState != null)
            {
                UiState.FilterSelectedPriority = Filter?.SelectedPriority ?? "All";
                UiState.FilterTag = Filter?.TagFilter ?? string.Empty;
                UiState.FilterCompletedStatus = Filter?.CompletedStatus ?? "All";
                UiState.LastSelectedTaskId = EditingTask?.Id;
            }
        }
        #endregion

        #region Private Methods
        private void UpdateFilteredTasks()
        {
            var view = CollectionViewSource.GetDefaultView(FilteredTasksView) as IEditableCollectionView;
            if (view != null)
            {
                if (view.IsAddingNew)
                    view.CommitNew();
                if (view.IsEditingItem)
                    view.CommitEdit();
            }
            FilteredTasksView.Refresh();
            // Update filtered overview
            var filtered = Filter.Apply(Tasks, EditingTask);
            FilteredOverview.Refresh(filtered, EditingTask);
            Overview.Refresh(Tasks);
        }

        private bool FilterPredicate(object obj)
        {
            var t = obj as TaskModel;
            if (t == null) return false;
            // Always include the currently edited task
            if (EditingTask != null && t == EditingTask)
                return true;
            return Filter.Apply(new[] { t }).Any();
        }

        private void ChangeTaskEditMode(TaskModel task, bool isEditing)
        {
            task.IsEditing = isEditing;
            EditingTask = isEditing ? task : null;
            EditingTaskBackup = isEditing ? task.Clone() : null;
        }

        private async Task LoadTasksAsync()
        {
            IsLoading = true;
            AddTaskErrorMessage = string.Empty;
            await RunWithErrorHandlingAsync(async () =>
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId > 0)
                {
                    var userTaskDtos = await _taskSyncClient!.GetUserTasksAsync(currentUserId);

                    var userTaskModels = userTaskDtos.Select(dto => _mapper!.Map<TaskModel>(dto));
                    Tasks = new ObservableCollection<TaskModel>(userTaskModels);
                    Overview.Refresh(Tasks);
                    FilteredTasksView = CollectionViewSource.GetDefaultView(Tasks);
                    FilteredTasksView.Filter = FilterPredicate;
                    UpdateFilteredTasks();
                }
                else
                {
                    _logger.Warning("No valid user ID found for loading tasks");
                    AddTaskErrorMessage = "User not authenticated. Please log in.";
                    SnackbarMessageQueue.Enqueue(AddTaskErrorMessage);
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
            UpdateFilteredTasks();
        }

        private void HandleTaskAdded(TaskDTO taskDto)
        {
            var taskModel = _mapper!.Map<TaskModel>(taskDto);
            Tasks.Add(taskModel);

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

                UpdateFilteredTasks();
            }
        }

        private void HandleTaskDeleted(int taskId)
        {
            var taskToRemove = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (taskToRemove != null)
            {
                Tasks.Remove(taskToRemove);

                UpdateFilteredTasks();
            }
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateFilteredTasks();
            UpdateUiStateFromCurrent();
        }

        partial void OnEditingTaskChanged(TaskModel value)
        {
            if (value != null && !value.IsEditing)
            {
                EditTaskCommand.Execute(value);
            }
            UpdateUiStateFromCurrent();
        }

        #endregion
    }
}

