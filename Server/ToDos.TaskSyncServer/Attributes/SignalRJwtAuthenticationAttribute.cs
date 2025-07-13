using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Security.Claims;
using System.Web;
using ToDos.JwtService;

namespace ToDos.TaskSyncServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SignalRJwtAuthenticationAttribute : AuthorizeAttribute
    {
        private readonly IJwtService _jwtService;

        public SignalRJwtAuthenticationAttribute()
        {
            // Get JWT service from dependency resolver
            _jwtService = GlobalHost.DependencyResolver.GetService(typeof(IJwtService)) as IJwtService;
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            try
            {
                // Get JWT token from query string or headers
                var token = GetTokenFromRequest(request);
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                // Validate token
                var principal = _jwtService?.ValidateToken(token);
                if (principal == null)
                {
                    return false;
                }

                // Store user information in request context for later use
                request.Environment["UserPrincipal"] = principal;
                request.Environment["UserId"] = _jwtService.GetUserIdFromToken(token).ToString();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            try
            {
                // Get user information from request context
                var request = hubIncomingInvokerContext.Hub.Context.Request;
                var principal = request.Environment["UserPrincipal"] as ClaimsPrincipal;
                var userId = request.Environment["UserId"] as string;

                if (principal == null || string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                // Store user information in hub context for method access
                hubIncomingInvokerContext.Hub.Context.Request.Environment["UserPrincipal"] = principal;
                hubIncomingInvokerContext.Hub.Context.Request.Environment["UserId"] = userId;
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetTokenFromRequest(IRequest request)
        {
            // Try to get token from query string first
            var token = request.QueryString["token"];
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            // Try to get token from Authorization header
            var authHeader = request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length);
            }

            return null;
        }
    }
} 