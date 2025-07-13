using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using Todos.Client.UserService.Interfaces;
using Todos.Client.UserService.Models;
using Todos.Ui.Models;
using Todos.Ui.Services.Navigation;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui.ViewModels
{
    public partial class ApplicationViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
        private ConnectionStatus connectionStatus;

        [ObservableProperty]
        private UserModel currentUser;

        private readonly IUserService _userService;

        public string ConnectionStatusText => ConnectionStatus switch
        {
            ConnectionStatus.Connected => "Connected",
            ConnectionStatus.Connecting => "Connecting...",
            ConnectionStatus.Reconnecting => "Reconnecting...",
            ConnectionStatus.Failed => "Connection Failed",
            _ => "Disconnected"
        };

        public ApplicationViewModel(IUserService userService, ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation) 
            : base(taskSyncClient, mapper, navigation)
        {
            _userService = userService;
            
            // Subscribe to events
            _userService.TokenChanged += HandleTokenChanged;
            _taskSyncClient.ConnectionStatusChanged += HandleConnectionStatusChanged;
            _userService.UserChanged += HandleUserChanged;
            
            // Initialize with current state
            CurrentUser = _mapper!.Map<UserModel>(_userService.CurrentUser);
            UpdateConnectionStatus();
        }

        private void HandleConnectionStatusChanged(ConnectionStatus newStatus)
        {
            ConnectionStatus = newStatus;
        }

        private async void HandleTokenChanged(string newToken)
        {
            // When token changes, update the task sync client
            if (!string.IsNullOrEmpty(newToken))
            {
                // TODO: Uncomment when JWT support is implemented in TaskSyncClient
                // _taskSyncClient.SetJwtToken(newToken);
                
                // For now, just connect without JWT
                try
                {
                    await _taskSyncClient.ConnectAsync();
                }
                catch (Exception)
                {
                    // Handle connection error
                    ConnectionStatus = ConnectionStatus.Failed;
                }
            }
            else
            {
                // Token cleared (logout), disconnect
                try
                {
                    await _taskSyncClient.DisconnectAsync();
                }
                catch (Exception)
                {
                    // Handle disconnection error
                }
            }
        }

        private void HandleUserChanged(UserDTO userDto)
        {
            CurrentUser = _mapper!.Map<UserModel>(userDto);
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                // Authenticate user first
                var authenticated = await _userService.AuthenticateAsync("defaultuser", "1234");
                if (!authenticated)
                {
                    ConnectionStatus = ConnectionStatus.Failed;
                }
                // Connection will be handled by HandleTokenChanged
            }
            catch (Exception)
            {
                // Handle connection error
                ConnectionStatus = ConnectionStatus.Failed;
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                // For now, use default credentials - in real app, this would come from login form
                var authenticated = await _userService.AuthenticateAsync("defaultuser", "1234");
                if (!authenticated)
                {
                    ConnectionStatus = ConnectionStatus.Failed;
                }
                // Connection will be handled by HandleTokenChanged
            }
            catch (Exception)
            {
                // Handle login error
                ConnectionStatus = ConnectionStatus.Failed;
            }
        }

        [RelayCommand]
        private async Task DisconnectAsync()
        {
            try
            {
                await _userService.LogoutAsync();
                // Disconnection will be handled by HandleTokenChanged
            }
            catch (Exception)
            {
                // Handle disconnection error
                ConnectionStatus = ConnectionStatus.Failed;
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                await _userService.LogoutAsync();
                // Navigation will be handled by HandleUserChanged
            }
            catch (Exception)
            {
                // Handle logout error
                ConnectionStatus = ConnectionStatus.Failed;
            }
        }

        private void UpdateConnectionStatus()
        {
            ConnectionStatus = _taskSyncClient.ConnectionStatus;
        }
    }
}
