using System;

namespace Todos.Client.UserService.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
} 