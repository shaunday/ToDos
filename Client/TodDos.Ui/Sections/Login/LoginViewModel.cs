using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodDos.Ui.Global.ViewModels;
using Todos.Ui.Services.Navigation;
using Serilog;
using Unity;

namespace Todos.Ui.ViewModels
{
    public partial class LoginViewModel : ViewModelBase, IInitializable, ICleanable
    {

        public LoginViewModel(INavigationService navigation, ILogger logger = null) : base(navigation, logger)
        {
        }

        public override void Init() { }
        public override void Cleanup() { }

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [RelayCommand]
        private void Login()
        {
            // Authentication is intentionally mocked for this project. Replace with real authentication logic if needed in production.
            if (!string.IsNullOrWhiteSpace(Username) && Password == "1234")
            {
                Navigation.NavigateTo<TasksViewModel>();
            }
            else
            {
                // You can add a property like ErrorMessage and bind to a TextBlock
            }
        }
    }

}
