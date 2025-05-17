using System.Text.Json;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public static class UserSessionHelper
    {
        private const string SESSION_KEY = "CurrentUser";

        public class UserSession
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }

        public static void SetUserSession(HttpContext httpContext, User user)
        {
            var sessionUser = new UserSession
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            httpContext.Session.SetString(SESSION_KEY, JsonSerializer.Serialize(sessionUser));
        }

        public static UserSession? GetUserSession(HttpContext httpContext)
        {
            var sessionData = httpContext.Session.GetString(SESSION_KEY);
            if (string.IsNullOrEmpty(sessionData))
                return null;

            try
            {
                return JsonSerializer.Deserialize<UserSession>(sessionData);
            }
            catch
            {
                return null;
            }
        }

        public static void ClearUserSession(HttpContext httpContext)
        {
            httpContext.Session.Remove(SESSION_KEY);
        }

        public static bool IsAuthenticated(HttpContext httpContext)
        {
            return GetUserSession(httpContext) != null;
        }

        public static int? GetUserId(HttpContext httpContext)
        {
            var session = GetUserSession(httpContext);
            return session?.Id;
        }
    }
} 