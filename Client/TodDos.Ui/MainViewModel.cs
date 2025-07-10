using CommunityToolkit.Mvvm.ComponentModel;
using ToDos.Ui.Services.Navigation;

namespace ToDos.Ui
{
    public class MainViewModel : ObservableObject
    {
        public INavigationService _navigation { get; }

        public MainViewModel(INavigationService navigation)
        {
            _navigation = navigation;
        }
    }

}