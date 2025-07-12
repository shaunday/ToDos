using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using Todos.Ui.Models;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui.ViewModels
{
    public partial class ApplicationViewModel : ObservableObject
    {
        [ObservableProperty]
        private ConnectionStatus connectionStatus;

        [ObservableProperty]
        private string connectionStatusText = "Disconnected";

        [ObservableProperty]
        private UserModel currentUser;

        private readonly ITaskSyncClient _taskSyncClient;

        public ApplicationViewModel(ITaskSyncClient taskSyncClient) 
        {
            _taskSyncClient = taskSyncClient;
            CurrentUser = new UserModel();
            
            // Subscribe to connection status changes
            _taskSyncClient.ConnectionStatusChanged += HandleConnectionStatusChanged;
            
            // Initialize connection status
            UpdateConnectionStatus();
        }

        private void HandleConnectionStatusChanged(ConnectionStatus newStatus)
        {
            ConnectionStatus = newStatus;
            UpdateConnectionStatus();
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                await _taskSyncClient.ConnectAsync();
            }
            catch (Exception)
            {
                // Handle connection error
                ConnectionStatusText = "Connection failed";
            }
        }

        [RelayCommand]
        private async Task DisconnectAsync()
        {
            try
            {
                await _taskSyncClient.DisconnectAsync();
            }
            catch (Exception)
            {
                // Handle disconnection error
                ConnectionStatusText = "Disconnection failed";
            }
        }

        private void UpdateConnectionStatus()
        {
            ConnectionStatus = _taskSyncClient.ConnectionStatus;
            ConnectionStatusText = ConnectionStatus switch
            {
                ConnectionStatus.Connected => "Connected",
                ConnectionStatus.Connecting => "Connecting...",
                ConnectionStatus.Reconnecting => "Reconnecting...",
                ConnectionStatus.Failed => "Connection Failed",
                _ => "Disconnected"
            };
        }
    }
}
