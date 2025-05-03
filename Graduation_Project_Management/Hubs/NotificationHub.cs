using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Graduation_Project_Management.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();

        public Task RegisterUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }
            _userConnections[userId] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        public static string? GetConnectionId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }
    }
}