using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Todos.Ui.Models
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

        [ObservableProperty]
        private DateTime? lastLoginTime;

        public UserModel()
        {
            // Default values for guest user
            Id = Guid.Empty.ToString();
            UserName = "Guest User";
            Email = "guest@example.com";
            Role = "Guest";
            DisplayName = "Guest User";
            IsAuthenticated = false;
            LastLoginTime = null;
        }
    }
} 