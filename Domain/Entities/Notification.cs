using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum NotificationType
    {
        JoinRequest,
        JoinRequestResponse,
        ProjectIdeaRequest
    }
    public enum NotificationStatus
    {
        Unread,
        Read
    }

    public class Notification
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string RecipientId { get; set; }
        public AppUser Recipient { get; set; } // Navigation to AspNetUsers

        public string? SenderId { get; set; }
        public AppUser? Sender { get; set; } // Optional navigation

        public NotificationType Type { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
