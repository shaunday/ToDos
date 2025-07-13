using CommunityToolkit.Mvvm.ComponentModel;
using Todos.Client.Common.Interfaces;
using Todos.Client.UserService.Interfaces;
using Todos.Ui.Services.Navigation;
using TodDos.Ui.Global.ViewModels;
using Todos.Ui.ViewModels;
using AutoMapper;

namespace Todos.Ui
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ApplicationViewModel applicationViewModel;

        public MainViewModel(INavigationService navigation, IUserService userService, ITaskSyncClient taskSyncClient, IMapper mapper) 
            : base(mapper, navigation)
        {
            applicationViewModel = new ApplicationViewModel(userService, taskSyncClient, mapper, navigation);
            Navigation.NavigateTo<TasksViewModel>();
        }
    }
}