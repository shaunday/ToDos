using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Ui.ViewModels;
using Unity;

namespace Todos.Ui.Services.Navigation
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IUnityContainer _container;

        public NavigationService(IUnityContainer container)
        {
            _container = container;
        }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentViewModel = _container.Resolve<TViewModel>();
        }
    }

}
