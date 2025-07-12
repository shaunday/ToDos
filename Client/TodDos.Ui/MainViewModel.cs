using CommunityToolkit.Mvvm.ComponentModel;
using Todos.Client.Common.Interfaces;
using Todos.Client.UserService.Interfaces;
using Todos.Ui.Services.Navigation;
using Todos.Ui.ViewModels;

namespace Todos.Ui
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ApplicationViewModel applicationViewModel;

        public MainViewModel(INavigationService navigation, IUserService userService, ITaskSyncClient taskSyncClient) : base(navigation)
        {
            applicationViewModel = new ApplicationViewModel(userService, taskSyncClient, navigation);
            Navigation.NavigateTo<TasksViewModel>();
        }
    }
}