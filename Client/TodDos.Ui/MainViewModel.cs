using CommunityToolkit.Mvvm.ComponentModel;
using Todos.Client.Common.Interfaces;
using Todos.Client.UserService.Interfaces;
using Todos.Ui.Services.Navigation;
using TodDos.Ui.Global.ViewModels;
using Todos.Ui.ViewModels;
using AutoMapper;
using System.Threading.Tasks;
using System.Windows;
using System;
using Serilog;
using Unity;

namespace Todos.Ui
{
    public partial class MainViewModel : ViewModelBase
    {
        public MainViewModel(INavigationService navigation, Serilog.ILogger logger = null) : base(navigation, logger)
        {
            // DEV CODE: Auto-login for development/testing
            _ = Task.Run(async () =>
            {
                try
                {
                    var userService = App.Container.Resolve<Todos.Client.UserService.Interfaces.IUserService>();
                    var result = await userService.AuthenticateAsync("defaultuser", "1234");
                    if (result)
                    {
                        // Navigate to tasks after successful login
                        await Task.Delay(100); // Small delay to ensure login completes
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Navigation.NavigateTo<TasksViewModel>();
                        }));
                    }
                }
                catch (Exception )
                {
                    // Handle auto-login failure silently
                }
            });
        }
    }
}