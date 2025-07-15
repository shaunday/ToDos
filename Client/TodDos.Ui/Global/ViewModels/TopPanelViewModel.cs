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
using Todos.Ui.Services;
using Todos.Ui.Services.Navigation;
using static Todos.Client.Common.TypesGlobal;
using TodDos.Ui.Global.ViewModels;
using Todos.Client.TaskSyncWithOfflineQueues;
using Unity;

namespace Todos.Ui.ViewModels
{
    public partial class TopPanelViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
        private ConnectionStatus connectionStatus;

        [ObservableProperty]
        private UserModel currentUser;

        private readonly UserConnectionService _userConnectionService;

        public string ConnectionStatusText => ConnectionStatus switch
        {
            ConnectionStatus.Connected => "Connected",
            ConnectionStatus.Connecting => "Connecting...",
            ConnectionStatus.Reconnecting => "Reconnecting...",
            ConnectionStatus.Failed => "Connection Failed",
            _ => "Disconnected"
        };

        public TopPanelViewModel(UserConnectionService userConnectionService, INavigationService navigation)
            : base(navigation)
        {
            _userConnectionService = userConnectionService;
            _userConnectionService.UserChanged += OnUserChanged;
            _userConnectionService.ConnectionStatusChanged += HandleConnectionStatusChanged;
            CurrentUser = _userConnectionService.CurrentUser;
            ConnectionStatus = _userConnectionService.ConnectionStatus;
        }

        private void OnUserChanged(UserModel user)
        {
            CurrentUser = user;
        }

        private void HandleConnectionStatusChanged(ConnectionStatus status)
        {
            ConnectionStatus = status;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            return await _userConnectionService.AuthenticateAsync(username, password);
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                await _userConnectionService.LogoutAsync();
                // Disconnection will be handled by HandleTokenChanged
            }
            catch (Exception)
            {
                // Handle disconnection error
                ConnectionStatus = ConnectionStatus.Failed;
            }
        }



        private void UpdateConnectionStatus()
        {
            ConnectionStatus = _taskSyncClient.ConnectionStatus;
        }
    }
}
