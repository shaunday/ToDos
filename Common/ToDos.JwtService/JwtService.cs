using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using ToDos.DotNet.Common;

namespace ToDos.JwtService
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string username);
        ClaimsPrincipal ValidateToken(string token);
        int GetUserIdFromToken(string token);
        int GetUserIdFromTokenWithoutValidation(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly ILogger _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(ILogger logger)
        {
            _logger = logger;
            
            // Load JWT configuration from environment variables
            _secretKey = Environment.GetEnvironmentVariable(Globals.JWT_Token_String_Identifier) ?? "default_secret_key_for_development_only";
            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ToDos.Server";
            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ToDos.Client";
            
            _logger.Information("JWT Service initialized with issuer: {Issuer}, audience: {Audience}", _issuer, _audience);
        }

        public string GenerateToken(int userId, string username)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Name, username),
                        new Claim("userId", userId.ToString()),
                        new Claim("username", username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(24), // 24 hour expiration
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key), 
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                
                _logger.Information("Generated JWT token for user: {Username} (ID: {UserId})", username, userId);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating JWT token for user: {Username} (ID: {UserId})", username, userId);
                throw;
            }
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                _logger.Debug("JWT token validated successfully");
                return principal;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "JWT token validation failed");
                return null;
            }
        }

        public int GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null) return 0;

                var userIdClaim = principal.FindFirst("userId") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting user ID from JWT token");
                return 0;
            }
        }

        public int GetUserIdFromTokenWithoutValidation(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.Warning("No JWT token provided for extracting user ID");
                    return 0;
                }

                // Extract user ID from JWT token without validation
                // This is useful for client-side parsing where we don't need to validate the signature
                var tokenParts = token.Split('.');
                if (tokenParts.Length >= 2)
                {
                    // Decode the payload (second part)
                    var payload = tokenParts[1];
                    // Add padding if needed
                    payload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                    
                    // Convert from base64url to base64
                    payload = payload.Replace('-', '+').Replace('_', '/');
                    
                    // Decode
                    var jsonBytes = Convert.FromBase64String(payload);
                    var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                    
                    // Simple JSON parsing to get userId
                    // In a real app, use a proper JSON library
                    var userIdMatch = System.Text.RegularExpressions.Regex.Match(json, @"""userId"":\s*(\d+)");
                    if (userIdMatch.Success && int.TryParse(userIdMatch.Groups[1].Value, out var userId))
                    {
                        return userId;
                    }
                }
                
                _logger.Warning("Could not extract user ID from JWT token");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting user ID from JWT token without validation");
                return 0;
            }
        }
    }
} 