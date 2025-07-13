using System;
using System.Collections.Generic;
using Serilog;
using ToDos.DotNet.Common;

namespace ToDos.MockAuthService
{
    public interface IAuthService
    {
        string GenerateToken(int userId, string username);
        bool ValidateToken(string token);
        int GetUserIdFromToken(string token);
        int GetUserIdFromTokenWithoutValidation(string token);
    }

    public class MockAuthService : IAuthService
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, TokenInfo> _validTokens = new Dictionary<string, TokenInfo>();

        public MockAuthService(ILogger logger)
        {
            _logger = logger;
            _logger.Information("Mock Auth Service initialized");
        }

        public string GenerateToken(int userId, string username)
        {
            try
            {
                // Create a simple mock token format: "MOCK_{userId}_{username}_{timestamp}"
                var timestamp = DateTime.UtcNow.Ticks;
                var token = $"MOCK_{userId}_{username}_{timestamp}";
                
                // Store token info for validation
                _validTokens[token] = new TokenInfo
                {
                    UserId = userId,
                    Username = username,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
                
                _logger.Information("Generated mock token for user: {Username} (ID: {UserId})", username, userId);
                return token;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating mock token for user: {Username} (ID: {UserId})", username, userId);
                throw;
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.Warning("No token provided for validation");
                    return false;
                }

                if (!_validTokens.TryGetValue(token, out var tokenInfo))
                {
                    _logger.Warning("Token not found in valid tokens");
                    return false;
                }

                if (DateTime.UtcNow > tokenInfo.ExpiresAt)
                {
                    _logger.Warning("Token has expired");
                    _validTokens.Remove(token);
                    return false;
                }

                _logger.Debug("Mock token validated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Mock token validation failed");
                return false;
            }
        }

        public int GetUserIdFromToken(string token)
        {
            try
            {
                if (!ValidateToken(token))
                {
                    return 0;
                }

                if (_validTokens.TryGetValue(token, out var tokenInfo))
                {
                    return tokenInfo.UserId;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting user ID from mock token");
                return 0;
            }
        }

        public int GetUserIdFromTokenWithoutValidation(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.Warning("No token provided for extracting user ID");
                    return 0;
                }

                // Parse mock token format: "MOCK_{userId}_{username}_{timestamp}"
                if (token.StartsWith("MOCK_"))
                {
                    var parts = token.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out var userId))
                    {
                        return userId;
                    }
                }

                // Fallback: try to get from stored tokens
                if (_validTokens.TryGetValue(token, out var tokenInfo))
                {
                    return tokenInfo.UserId;
                }

                _logger.Warning("Could not extract user ID from mock token");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting user ID from mock token without validation");
                return 0;
            }
        }

        private class TokenInfo
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
} 