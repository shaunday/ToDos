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
            // Call Cleanup on the old viewmodel if it implements ICleanable
            if (CurrentViewModel is ICleanable cleanable)
            {
                cleanable.Cleanup();
            }
            var newViewModel = _container.Resolve<TViewModel>();
            // Call Init on the new viewmodel if it implements IInitializable
            if (newViewModel is IInitializable initializable)
            {
                initializable.Init();
            }
            CurrentViewModel = newViewModel;
        }
    }

}
