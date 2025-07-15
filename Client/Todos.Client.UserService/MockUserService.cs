using System;
using System.Threading.Tasks;
using DotNetEnv;
using Todos.Client.UserService.Interfaces;
using Todos.Client.UserService.Models;
using ToDos.MockAuthService;

namespace Todos.UserService
{
    public class MockUserService : IUserService
    {
        private UserDTO _currentUser;
        private string _jwtToken;
        private readonly IAuthService _authService;
        
        public UserDTO CurrentUser => _currentUser;
        public string JwtToken => _jwtToken;
        public event Action<UserDTO> UserChanged;
        public event Action<string> TokenChanged;
        
        public MockUserService()
        {
            // Load environment variables from .env.Global file
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            Env.Load(envPath);
            
            // Initialize Auth service
            _authService = new MockJwtAuthService(Serilog.Log.Logger);
            
            // Initialize with guest user
            _currentUser = new UserDTO
            {
                Id = 0,
                UserName = "Guest",
                Email = string.Empty,
                Role = "Guest",
                DisplayName = "Guest User",
                IsAuthenticated = false,
                LastLoginTime = null
            };
            _jwtToken = string.Empty;
        }
        
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            // Simulate async operation
            await Task.Delay(100);
            
            // Simple mock authentication - accept any non-empty username with password "1234"
            if (!string.IsNullOrWhiteSpace(username) && password == "1234")
            {
                // Use fixed user IDs that match our mock data
                var userId = username.ToLower() switch
                {
                    "user1" => 1,
                    "defaultuser" => 1,
                    "user2" => 2,
                    _ => 1 // Default to user 1 for any other username
                };
                
                var user = new UserDTO
                {
                    Id = userId,
                    UserName = username,
                    Email = $"{username}@example.com",
                    Role = "User",
                    DisplayName = username,
                    IsAuthenticated = true,
                    LastLoginTime = DateTime.UtcNow
                };
                
                // Generate proper mock token using the service
                var mockToken = _authService.GenerateToken(user.Id, user.UserName);
                
                _currentUser = user;
                _jwtToken = mockToken;
                
                UserChanged?.Invoke(user);
                TokenChanged?.Invoke(mockToken);
                return true;
            }
            
            return false;
        }

        public async Task LogoutAsync()
        {
            // Simulate async operation
            await Task.Delay(50);
            
            var guestUser = new UserDTO
            {
                Id = 0,
                UserName = "Guest",
                Email = string.Empty,
                Role = "Guest",
                DisplayName = "Guest User",
                IsAuthenticated = false,
                LastLoginTime = null
            };
            
            var oldToken = _jwtToken;
            
            _currentUser = guestUser;
            _jwtToken = string.Empty;
            
            UserChanged?.Invoke(guestUser);
            TokenChanged?.Invoke(string.Empty);
        }
        
        public async Task<bool> RefreshTokenAsync()
        {
            // Simulate async operation
            await Task.Delay(200);
            
            if (_currentUser.IsAuthenticated)
            {
                // Generate new mock token using the service
                var newToken = _authService.GenerateToken(_currentUser.Id, _currentUser.UserName);
                _jwtToken = newToken;
                TokenChanged?.Invoke(newToken);
                return true;
            }
            
            return false;
        }
    }
} 