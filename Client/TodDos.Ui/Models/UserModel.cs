using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Todos.Ui.Models
{
    public partial class UserModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

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
    }
}