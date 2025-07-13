using System;
using System.Threading.Tasks;
using Todos.Client.UserService.Models;

namespace Todos.Client.UserService.Interfaces
{
    public interface IUserService
    {
        UserDTO CurrentUser { get; }
        string JwtToken { get; }
        event Action<UserDTO> UserChanged;
        event Action<string> TokenChanged;
        
        Task<bool> AuthenticateAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();
    }
} 