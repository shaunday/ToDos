using CommunityToolkit.Mvvm.ComponentModel;
using Todos.Ui.Services.Navigation;
using Todos.Ui.ViewModels;
using Todos.Client.Common.Interfaces;

namespace Todos.Ui
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ApplicationViewModel applicationViewModel;

        public MainViewModel(INavigationService navigation, ITaskSyncClient taskSyncClient) : base(navigation)
        {
            applicationViewModel = new ApplicationViewModel(taskSyncClient);
            Navigation.NavigateTo<TasksViewModel>();
        }
    }
}