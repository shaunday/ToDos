using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Todos.Client.UserService.Models
{
    public partial class UserModel : ObservableObject
    {
        [ObservableProperty]
        private string id;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string role;

        [ObservableProperty]
        private string displayName;

        [ObservableProperty]
        private bool isAuthenticated;

        public UserModel() : this("Guest User") { }

        public UserModel(string username)
        {
            Id = Guid.NewGuid().ToString();
            UserName = username;
            Email = $"{username}@example.com";
            Role = "User";
            DisplayName = username;
            IsAuthenticated = true;
        }
    }
}