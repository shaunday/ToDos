using System;
using System.Threading.Tasks;
using AutoMapper;
using Todos.Client.Common.Interfaces;
using Todos.Client.UserService.Interfaces;
using Todos.Client.UserService.Models;
using Todos.Ui.Models;
using Todos.Client.TaskSyncWithOfflineQueues;
using Unity;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui.Services
{
    public class UserConnectionService
    {
        public event Action<UserModel> UserChanged;
        public event Action<ConnectionStatus> ConnectionStatusChanged;
        public UserModel CurrentUser { get; private set; }
        public ConnectionStatus ConnectionStatus { get; private set; }

        private readonly IUserService _userService;
        private readonly ITaskSyncClient _taskSyncClient;
        private readonly IMapper _mapper;
        private readonly IUnityContainer _container;

        public UserConnectionService(IUserService userService, ITaskSyncClient taskSyncClient, IMapper mapper, IUnityContainer container)
        {
            _userService = userService;
            _taskSyncClient = taskSyncClient;
            _mapper = mapper;
            _container = container;

            _userService.TokenChanged += HandleTokenChanged;
            _taskSyncClient.ConnectionStatusChanged += HandleConnectionStatusChanged;
            _userService.UserChanged += HandleUserChanged;

            CurrentUser = _mapper.Map<UserModel>(_userService.CurrentUser);
            ConnectionStatus = _taskSyncClient.ConnectionStatus;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            return await _userService.AuthenticateAsync(username, password);
        }

        public async Task LogoutAsync()
        {
            await _userService.LogoutAsync();
        }

        private void HandleConnectionStatusChanged(ConnectionStatus newStatus)
        {
            ConnectionStatus = newStatus;
            ConnectionStatusChanged?.Invoke(newStatus);
        }

        private async void HandleTokenChanged(string newToken)
        {
            if (!string.IsNullOrEmpty(newToken))
            {
                _taskSyncClient.SetJwtToken(newToken);
                try { await _taskSyncClient.ConnectAsync(); }
                catch { ConnectionStatus = ConnectionStatus.Failed; ConnectionStatusChanged?.Invoke(ConnectionStatus.Failed); }
            }
            else
            {
                try { await _taskSyncClient.DisconnectAsync(); }
                catch { }
                ClearOfflineQueue();
            }
        }

        private void HandleUserChanged(UserDTO userDto)
        {
            CurrentUser = _mapper.Map<UserModel>(userDto);
            UserChanged?.Invoke(CurrentUser);
            ClearOfflineQueue();
        }

        private void ClearOfflineQueue()
        {
            var offlineQueueService = (_container as UnityContainer)?.Resolve<IOfflineQueueService>();
            offlineQueueService?.Clear();
        }
    }
} 