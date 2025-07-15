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

namespace Todos.Ui
{
    public partial class MainViewModel : ViewModelBase
    {
        // Remove direct construction of TopPanelViewModel and any unused field.
        public MainViewModel(INavigationService navigation, IUserService userService, ITaskSyncClient taskSyncClient, IMapper mapper) 
            : base(mapper, navigation)
        {
            // DEV CODE: Auto-login for development/testing
            _ = Task.Run(async () =>
            {
                try
                {
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
                catch (Exception ex)
                {
                    // Handle auto-login failure silently
                }
            });
        }
    }
}