using System;
using System.Threading.Tasks;
using DotNetEnv;
using Todos.Client.UserService.Interfaces;
using Todos.Client.UserService.Models;

namespace Todos.Client.UserService
{
    public class MockUserService : IUserService
    {
        private UserDTO _currentUser;
        private string _jwtToken;
        private string _jwtTokenPrefix;
        
        public UserDTO CurrentUser => _currentUser;
        public string JwtToken => _jwtToken;
        public event Action<UserDTO> UserChanged;
        public event Action<string> TokenChanged;
        
        public MockUserService()
        {
            // Load environment variables from .env.Global file
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            Env.Load(envPath);
            _jwtTokenPrefix = Environment.GetEnvironmentVariable("JWT_TOKEN_PREFIX");
            
            if (string.IsNullOrEmpty(_jwtTokenPrefix))
            {
                throw new InvalidOperationException("JWT_TOKEN_PREFIX environment variable not found in .env.Global file");
            }
            
            // Initialize with guest user
            _currentUser = CreateGuestUserDTO();
            _jwtToken = string.Empty;
        }
        
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            // Simulate async operation
            await Task.Delay(100);
            
            // Simple mock authentication - accept any non-empty username with password "1234"
            if (!string.IsNullOrWhiteSpace(username) && password == "1234")
            {
                var user = CreateAuthenticatedUserDTO(username);
                
                // Generate mock JWT token
                var mockToken = $"{_jwtTokenPrefix}{username}_{DateTime.Now.Ticks}";
                
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
            
            var guestUser = CreateGuestUserDTO();
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
                // Generate new mock JWT token
                var newToken = $"{_jwtTokenPrefix}{_currentUser.UserName}_{DateTime.Now.Ticks}_refreshed";
                _jwtToken = newToken;
                TokenChanged?.Invoke(newToken);
                return true;
            }
            
            return false;
        }

        private static UserDTO CreateGuestUserDTO()
        {
            return new UserDTO
            {
                Id = Guid.Empty.ToString(),
                UserName = "Guest User",
                Email = "guest@example.com",
                Role = "Guest",
                DisplayName = "Guest User",
                IsAuthenticated = false,
                LastLoginTime = null
            };
        }

        private static UserDTO CreateAuthenticatedUserDTO(string username)
        {
            return new UserDTO
            {
                Id = Guid.NewGuid().ToString(),
                UserName = username,
                Email = $"{username}@example.com",
                Role = "User",
                DisplayName = username,
                IsAuthenticated = true,
                LastLoginTime = DateTime.Now
            };
        }
    }
} 