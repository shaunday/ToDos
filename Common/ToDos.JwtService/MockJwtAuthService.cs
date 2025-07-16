using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ToDos.DotNet.Common;

namespace ToDos.MockAuthService
{
    public interface IAuthService
    {
        string GenerateToken(int userId);
        bool ValidateToken(string token);
        int GetUserIdFromToken(string token);
        int GetUserIdFromTokenWithoutValidation(string token);
    }

    public class MockJwtAuthService : IAuthService
    {
        private readonly ILogger _logger;

        public MockJwtAuthService(ILogger logger)
        {
            _logger = logger;
            _logger.Information("Mock Auth Service initialized");
        }

        public string GenerateToken(int userId)
        {
            string username = "mock";
            try
            {
                // Create a simple mock token format: "MOCK_{userId}_{username}_{expiresAtTicks}"
                var expiresAt = DateTime.UtcNow.AddHours(24);
                var token = $"MOCK_{userId}_{username}_{expiresAt.Ticks}";
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

                // Parse mock token format: "MOCK_{userId}_{username}_{expiresAtTicks}"
                if (!token.StartsWith("MOCK_"))
                {
                    _logger.Warning("Token does not match mock format");
                    return false;
                }
                var parts = token.Split('_');
                if (parts.Length < 4)
                {
                    _logger.Warning("Token does not have enough parts");
                    return false;
                }
                if (!long.TryParse(parts[3], out var expiresAtTicks))
                {
                    _logger.Warning("Token expiration is not valid");
                    return false;
                }
                var expiresAt = new DateTime(expiresAtTicks, DateTimeKind.Utc);
                if (DateTime.UtcNow > expiresAt)
                {
                    _logger.Warning("Token has expired");
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
                // Parse mock token format: "MOCK_{userId}_{username}_{expiresAtTicks}"
                var parts = token.Split('_');
                if (parts.Length >= 3 && int.TryParse(parts[1], out var userId))
                {
                    return userId;
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
                // Parse mock token format: "MOCK_{userId}_{username}_{expiresAtTicks}"
                if (token.StartsWith("MOCK_"))
                {
                    var parts = token.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out var userId))
                    {
                        return userId;
                    }
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
    }
} 