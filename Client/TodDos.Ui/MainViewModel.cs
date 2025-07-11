using CommunityToolkit.Mvvm.ComponentModel;
using ToDos.Ui.Services.Navigation;
using ToDos.Ui.ViewModels;

namespace ToDos.Ui
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(INavigationService navigation) :base(navigation)
        {
            _navigation.NavigateTo<TasksViewModel>();
        }
    }

}