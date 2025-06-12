using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface INotificationService
    {
        Task<ActionResult> GetUserNotificationsAsync(ClaimsPrincipal user);

    }
}
