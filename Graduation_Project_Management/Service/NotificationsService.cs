using Domain.Entities.Identity;
using Domain.Entities;
using Graduation_Project_Management.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Domain.Repository;
using Graduation_Project_Management.DTOs.NotificationsDto;
using Microsoft.EntityFrameworkCore;
using Graduation_Project_Management.IServices;

namespace Graduation_Project_Management.Service
{
    public class NotificationsService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
         
        public NotificationsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        

        public async Task<ActionResult> GetUserNotificationsAsync(ClaimsPrincipal user)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // Get the user by email
            var appUser = await _unitOfWork.GetRepository<AppUser>()
                .GetAllAsync()
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (appUser == null)
                return new NotFoundObjectResult(new ApiResponse(404, "User not found."));

            var notifications = await _unitOfWork.GetRepository<Notification>()
                .GetAllAsync()
                .Where(n => n.RecipientId == appUser.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var result = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                Status= n.Status.ToString(),
            }).ToList();

            return new OkObjectResult(result);
        }
        public async Task<ActionResult> MarkNotificationAsReadAsync(int notificationId, ClaimsPrincipal user)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // Get the user by email
            var appUser = await _unitOfWork.GetRepository<AppUser>()
                .GetAllAsync()
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (appUser == null)
                return new NotFoundObjectResult(new ApiResponse(404, "User not found."));

            // Get the notification
            var notification = await _unitOfWork.GetRepository<Notification>()
                .GetAllAsync()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == appUser.Id);

            if (notification == null)
                return new OkObjectResult(new ApiResponse(404, "Notification not found "));

            // Update notification status
            if (notification.Status != NotificationStatus.Read)
            {
                notification.Status = NotificationStatus.Read;
                await _unitOfWork.GetRepository<Notification>().UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync();
            }

            return new OkObjectResult(new ApiResponse(200, "Notification marked as read."));
        }
    }

}

