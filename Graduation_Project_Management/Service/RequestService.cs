using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;
using Graduation_Project_Management.Hubs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Service
{
    public class RequestService : IRequestService
    {

        #region Dependencies
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _notificationHub;


        public RequestService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> notificationHub)
        {
            _unitOfWork = unitOfWork;
            _notificationHub = notificationHub;

        }
        #endregion

        #region Get 
        public async Task<ActionResult> GetTeamJoinRequestsAsync(ClaimsPrincipal user, int teamId)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student?.TeamId != teamId)
                return new ObjectResult("You are not part of this team") { StatusCode = StatusCodes.Status403Forbidden };

            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.JoinRequests)
                .ThenInclude(r => r.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return new NotFoundObjectResult("Team not Found");

            var requests = team.JoinRequests?.Select(r => new
            {
                r.Id,
                r.StudentId,
                StudentName = r.Student.User.UserName,
                r.Message,
                Status = Enum.GetName(typeof(JoinRequestStatus), r.Status),
                r.CreatedAt
            }).ToList();

            return new OkObjectResult(requests);
        }
        #endregion

        #region Respond to Join Request
        public async Task<ActionResult> RespondToJoinRequestAsync(ClaimsPrincipal user, int requestId, string decision)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s => s.Team)
                .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student?.Team == null)
                return new ObjectResult("You are not part of this team") { StatusCode = StatusCodes.Status403Forbidden };

            var request = await _unitOfWork.GetRepository<TeamJoinRequest>().GetAllAsync()
                .Include(r => r.Team)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.TeamId != student.Team.Id)
                return new NotFoundObjectResult("Cannot Find The Request");

            if (request.Status != JoinRequestStatus.Pending)
                return new BadRequestObjectResult("Request has already been responded to.");

            string title = "";
            string content = "";
            Notification notification = null;

            try
            {
                if (decision.ToLower() == "accept")
                {
                    if (request.Team.TeamMembers?.Count >= request.Team.MaxMembers)
                        return new BadRequestObjectResult("Team is Full");

                    request.Status = JoinRequestStatus.Accepted;
                    request.Team.TeamMembers.Add(request.Student);

                    // Define notification
                    title = "Join Request Accepted";
                    content = $"Your request to join team '{request.Team.Name}' has been accepted.";
                    notification = new Notification
                    {
                        Message = content,
                        RecipientId = request.Student.UserId,
                        Type = NotificationType.JoinRequestResponse,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else if (decision.ToLower() == "reject")
                {
                    request.Status = JoinRequestStatus.Rejected;

                    // Define notification
                    title = "Join Request Rejected";
                    content = $"Your request to join team '{request.Team.Name}' has been rejected.";
                    notification = new Notification
                    {
                        Message = content,
                        RecipientId = request.Student.UserId,
                        Type = NotificationType.JoinRequestResponse,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    return new BadRequestObjectResult("The Request Must be Rejected Or Accepted");
                }

                // Log notification details
                Console.WriteLine($"Preparing notification: RecipientId={request.Student.UserId}, Title={title}, Content={content}");

                // Save notification to database
                await _unitOfWork.GetRepository<Notification>().AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine("Notification saved to database.");

                // Send notification via SignalR
                var connectionId = NotificationHub.GetConnectionId(request.Student.UserId);
                if (connectionId != null)
                {
                    await _notificationHub.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", title, content);
                    Console.WriteLine($"Notification sent to RecipientId={request.Student.UserId}, ConnectionId={connectionId}");
                }
                else
                {
                    Console.WriteLine($"No active connection found for RecipientId={request.Student.UserId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing notification: {ex.Message}");
                return new ObjectResult("An error occurred while processing the request") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return new OkObjectResult(new { message = $"The request has been {request.Status}" });
        }
        #endregion

        #region Request Supervisor
        public async Task<IActionResult> RequestSupervisorAsync(ClaimsPrincipal user, SendProjectIdeaRequestDto dto)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == email);

            if (student?.Team == null)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var idea = await _unitOfWork.GetRepository<ProjectIdea>().GetAllAsync()
                .FirstOrDefaultAsync(i => i.Id == dto.ProjectIdeaId && i.TeamId == student.Team.Id);
            if (idea == null)
                return new NotFoundObjectResult("Idea not found");

            var supervisor = await _unitOfWork.GetRepository<Supervisor>().GetByIdAsync(dto.SupervisorId);
            if (supervisor == null)
                return new NotFoundObjectResult("Supervisor not found");

            var request = new ProjectIdeaRequest
            {
                ProjectIdeaId = idea.Id,
                SupervisorId = dto.SupervisorId,
                Status = ProjectIdeaStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _unitOfWork.GetRepository<ProjectIdeaRequest>().AddAsync(request);

                // Send notification to the supervisor
                var title = "New Project Idea Request";
                var content = $"A project idea request has been sent from team '{student.Team.Name}' for your supervision.";
                var notification = new Notification
                {
                    Message = content,
                    RecipientId = supervisor.UserId,
                    Type = NotificationType.ProjectIdeaRequest,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.UtcNow
                };

                // Log notification details
                Console.WriteLine($"Preparing notification: RecipientId={supervisor.UserId}, Title={title}, Content={content}");

                // Save notification to database
                await _unitOfWork.GetRepository<Notification>().AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine("Notification saved to database.");

                // Send notification via SignalR
                var connectionId = NotificationHub.GetConnectionId(supervisor.UserId);
                if (connectionId != null)
                {
                    await _notificationHub.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", title, content);
                    Console.WriteLine($"Notification sent to RecipientId={supervisor.UserId}, ConnectionId={connectionId}");
                }
                else
                {
                    Console.WriteLine($"No active connection found for RecipientId={supervisor.UserId}");
                }

                return new OkObjectResult(new { message = "Request sent to supervisor" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing notification: {ex.Message}");
                return new ObjectResult("An error occurred while processing the request") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
        #endregion

        #region Join Team
        public async Task<ActionResult> RequestToJoinTeamAsync(ClaimsPrincipal user, TeamJoinRequestDto model)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student == null)
                return new NotFoundObjectResult("Student profile not found.");

            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.JoinRequests)
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId);

            if (team == null)
                return new NotFoundObjectResult("Team not found.");

            if (team.TeamMembers?.Any(m => m.Id == student.Id) == true)
                return new BadRequestObjectResult("You are already a member of this team.");

            if (team.JoinRequests?.Any(r => r.StudentId == student.Id) == true)
                return new BadRequestObjectResult("You already have a pending request to this team.");

            var joinRequest = new TeamJoinRequest
            {
                StudentId = student.Id,
                TeamId = team.Id,
                Message = model.Message,
                Status = JoinRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _unitOfWork.GetRepository<TeamJoinRequest>().AddAsync(joinRequest);

                // Send notification to team members
                var notificationsRepo = _unitOfWork.GetRepository<Notification>();
                foreach (var member in team.TeamMembers)
                {
                    var title = "New Team Join Request";
                    var content = $"A new request to join your team '{team.Name}' has been submitted by {student.FirstName}.";
                    var notification = new Notification
                    {
                        Message = content,
                        RecipientId = member.UserId,
                        Type = NotificationType.JoinRequest,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Log notification details
                    Console.WriteLine($"Preparing notification: RecipientId={member.UserId}, Title={title}, Content={content}");

                    // Save notification to database
                    await notificationsRepo.AddAsync(notification);
                    Console.WriteLine($"Notification saved for RecipientId={member.UserId}");

                    // Send notification via SignalR
                    var connectionId = NotificationHub.GetConnectionId(member.UserId);
                    if (connectionId != null)
                    {
                        await _notificationHub.Clients.Client(connectionId)
                            .SendAsync("ReceiveNotification", title, content);
                        Console.WriteLine($"Notification sent to RecipientId={member.UserId}, ConnectionId={connectionId}");
                    }
                    else
                    {
                        Console.WriteLine($"No active connection found for RecipientId={member.UserId}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine("All changes saved to database.");

                return new OkObjectResult(new { message = "Join request sent successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing join request: {ex.Message}");
                return new ObjectResult("An error occurred while processing the request") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        } 
        #endregion
    }
}
