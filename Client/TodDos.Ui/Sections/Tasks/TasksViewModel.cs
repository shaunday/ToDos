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
using System.Windows;
using Unity;
using Todos.Ui.Services;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui.ViewModels
{
    public partial class TasksViewModel : ViewModelBase, IInitializable, ICleanable
    {
        #region Fields
        private readonly IAuthService _authService;
        private readonly new IMapper _mapper;
        private readonly new ITaskSyncClient _taskSyncClient;
        private readonly UserConnectionService _userConnectionService;

        private ObservableCollection<TaskModel> Tasks { get; set; } = new ObservableCollection<TaskModel>();

        #endregion

        #region Properties

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

        #endregion

        #region Constructors and Lifecycle
        public TasksViewModel(INavigationService navigation, Serilog.ILogger logger, IAuthService authService)
            : base(navigation, logger)
        {
            _authService = authService;
            _mapper = App.Container.Resolve<IMapper>();
            _taskSyncClient = App.Container.Resolve<ITaskSyncClient>();
            _userConnectionService = App.Container.Resolve<UserConnectionService>();
            _userConnectionService.ConnectionStatusChanged += OnConnectionStatusChanged;
            FilteredTasksView = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasksView.Filter = FilterPredicate;
        }

        public override async void Init()
        {
            _logger?.Information("TasksViewModel: Init called");
            _taskSyncClient!.TaskAdded += HandleTaskAdded;
            _taskSyncClient.TaskUpdated += HandleTaskUpdated;
            _taskSyncClient.TaskDeleted += HandleTaskDeleted;
            _taskSyncClient.TaskLocked += HandleTaskLocked;
            _taskSyncClient.TaskUnlocked += HandleTaskUnlocked;
            Tasks.CollectionChanged += Tasks_CollectionChanged;
            Filter.PropertyChanged += Filter_PropertyChanged;

            await ReloadAllTasksAndUiStateAsync();
        }

        public override void Cleanup()
        {
            _logger?.Information("TasksViewModel: Cleanup called");
            _taskSyncClient!.TaskAdded -= HandleTaskAdded;
            _taskSyncClient.TaskUpdated -= HandleTaskUpdated;
            _taskSyncClient.TaskDeleted -= HandleTaskDeleted;
            _taskSyncClient.TaskLocked -= HandleTaskLocked;
            _taskSyncClient.TaskUnlocked -= HandleTaskUnlocked;
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
                Application.Current.Dispatcher.Invoke(() =>
                {
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
                });
            }, "Failed to delete task.", SnackbarMessageQueue);
            _ = _taskSyncClient!.DeleteTaskAsync(task.Id);
        }

        #endregion

        #region Public Methods
        public void ApplyUiState()
        {
            _logger?.Information("TasksViewModel: ApplyUiState called");
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
            _logger?.Information("TasksViewModel: UpdateUiStateFromCurrent called");
            if (UiState == null) return;
            UiState.FilterSelectedPriority = Filter?.SelectedPriority ?? "All";
            UiState.FilterTag = Filter?.TagFilter ?? string.Empty;
            UiState.FilterCompletedStatus = Filter?.CompletedStatus ?? "All";
            UiState.LastSelectedTaskId = EditingTask?.Id;
        }
        #endregion

        #region Private Methods
        private void UpdateFilteredTasks()
        {
            _logger?.Information("TasksViewModel: UpdateFilteredTasks called");
            if (CollectionViewSource.GetDefaultView(FilteredTasksView) is IEditableCollectionView view)
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
            if (!(obj is TaskModel t)) return false;
            // Always include the currently edited task
            if (EditingTask != null && t == EditingTask)
                return true;
            return Filter.Apply(new[] { t }).Any();
        }

        private void ChangeTaskEditMode(TaskModel task, bool isEditing)
        {
            _logger?.Information("TasksViewModel: ChangeTaskEditMode called for TaskId {TaskId}, isEditing: {IsEditing}", task?.Id, isEditing);
            task.IsEditing = isEditing;
            EditingTask = isEditing ? task : null;
            EditingTaskBackup = isEditing ? task.Clone() : null;
        }

        private async Task LoadTasksAsync()
        {
            _logger?.Information("TasksViewModel: LoadTasksAsync called");
            IsLoading = true;
            AddTaskErrorMessage = string.Empty;
            await RunWithErrorHandlingAsync(async () =>
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId > 0)
                {
                    var userTaskDtos = await _taskSyncClient!.GetUserTasksAsync(currentUserId);
                    var userTaskModels = userTaskDtos.Select(dto => _mapper!.Map<TaskModel>(dto));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Tasks = new ObservableCollection<TaskModel>(userTaskModels);
                        Overview.Refresh(Tasks);
                        FilteredTasksView = CollectionViewSource.GetDefaultView(Tasks);
                        FilteredTasksView.Filter = FilterPredicate;
                        UpdateFilteredTasks();
                    });
                }
                else
                {
                    _logger?.Warning("TasksViewModel: No valid user ID found for loading tasks");
                    AddTaskErrorMessage = "User not authenticated. Please log in.";
                    SnackbarMessageQueue.Enqueue(AddTaskErrorMessage);
                }
            }, "Failed to load tasks.", SnackbarMessageQueue);
            IsLoading = false;
        }

        private async Task ReloadAllTasksAndUiStateAsync()
        {
            await LoadTasksAsync();
            ApplyUiState();
        }

        private int GetCurrentUserId()
        {
            _logger?.Information("TasksViewModel: GetCurrentUserId called");
            var userConnectionService = App.Container.Resolve<UserConnectionService>();
            var currentUser = userConnectionService.CurrentUser;
            if (currentUser != null)
            {
                return currentUser.Id;
            }
            _logger?.Warning("No current user available in UserConnectionService");
            return 0;
        }

        private void OnConnectionStatusChanged(ConnectionStatus status)
        {
            if (status == ConnectionStatus.Connected)
            {
                // Reload all tasks and UI state on reconnect
                _ = ReloadAllTasksAndUiStateAsync();
            }
        }
        #endregion

        #region Event Handlers
        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _logger?.Information("TasksViewModel: Tasks_CollectionChanged called");
            UpdateFilteredTasks();
        }

        private void HandleTaskAdded(TaskDTO task)
        {
            _logger?.Information("TasksViewModel: HandleTaskAdded called for TaskId {TaskId}", task?.Id);
            if (task == null) return;
            var model = _mapper!.Map<TaskModel>(task);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Tasks.Add(model);
                UpdateFilteredTasks();
            });
        }

        private void HandleTaskUpdated(TaskDTO task)
        {
            _logger?.Information("TasksViewModel: HandleTaskUpdated called for TaskId {TaskId}", task?.Id);
            if (task == null) return;
            var model = Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (model != null)
            {
                var index = Tasks.IndexOf(model);
                var updatedModel = _mapper!.Map<TaskModel>(task);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Tasks[index] = updatedModel;
                    UpdateFilteredTasks();
                });
            }
        }

        private void HandleTaskDeleted(int taskId)
        {
            _logger?.Information("TasksViewModel: HandleTaskDeleted called for TaskId {TaskId}", taskId);
            var model = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (model != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Tasks.Remove(model);
                    Overview.Refresh(Tasks);
                });
            }
        }

        private void HandleTaskLocked(int taskId)
        {
            _logger?.Information("TasksViewModel: HandleTaskLocked called for TaskId {TaskId}", taskId);
            var model = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (model != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    model.IsLocked = true;
                    // If the currently edited task is now locked by someone else, exit edit mode
                    if (EditingTask != null && EditingTask.Id == taskId)
                    {
                        ChangeTaskEditMode(model, false);
                        AddTaskErrorMessage = "This task was locked by another user. Editing has been cancelled.";
                        SnackbarMessageQueue.Enqueue(AddTaskErrorMessage);
                    }
                    UpdateFilteredTasks();
                });
            }
        }

        private void HandleTaskUnlocked(int taskId)
        {
            _logger?.Information("TasksViewModel: HandleTaskUnlocked called for TaskId {TaskId}", taskId);
            var model = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (model != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    model.IsLocked = false;
                    UpdateFilteredTasks();
                });
            }
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _logger?.Information("TasksViewModel: Filter_PropertyChanged called for Property {PropertyName}", e?.PropertyName);
            UpdateFilteredTasks();
            UpdateUiStateFromCurrent();
        }

        partial void OnEditingTaskChanged(TaskModel value)
        {
            _logger?.Information("TasksViewModel: OnEditingTaskChanged called for TaskId {TaskId}", value?.Id);
            if (value != null && !value.IsEditing)
            {
                EditTaskCommand.Execute(value);
            }
            UpdateUiStateFromCurrent();
        }

        #endregion
    }
}

