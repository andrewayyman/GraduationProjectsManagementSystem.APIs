using Graduation_Project_Management.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Graduation_Project_Management.Service
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUser(string userId, string message)
        {
            var connectionId = NotificationHub.GetConnectionId(userId);
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", message);
            }
        }
    }
}

