using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;
using Graduation_Project_Management.Errors;
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
        private readonly ILogger<RequestService> _logger;

        public RequestService( IUnitOfWork unitOfWork, IHubContext<NotificationHub> notificationHub, ILogger<RequestService> logger )
        {
            _unitOfWork = unitOfWork;
            _notificationHub = notificationHub;
            _logger = logger;
        }
        #endregion

        #region RequestToJoinTeam
        public async Task<ActionResult> RequestToJoinTeamAsync(ClaimsPrincipal user, TeamJoinRequestDto model)
        {
            // get user
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // get student
            var student = await _unitOfWork.GetRepository<Student>()
                  .GetAllAsync()
                  .Include(s => s.Team)
                  .FirstOrDefaultAsync(s => s.Email == userEmail);



            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team != null )
                return new BadRequestObjectResult(new ApiResponse(400, "You are already a member of a team."));


            var team = await _unitOfWork.GetRepository<Team>()
                .GetAllAsync()
                .Include(t => t.JoinRequests)
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId);

            // Validate team
            if ( team == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Team not found."));
            if ( !team.IsOpenToJoin )
                return new BadRequestObjectResult(new ApiResponse(400, "Team is not open to join."));
            if ( team.TeamMembers?.Count >= team.MaxMembers )
                return new BadRequestObjectResult(new ApiResponse(400, "Team has reached its maximum member limit."));
            if ( team.JoinRequests?.Any(r => r.StudentId == student.Id && r.Status == JoinRequestStatus.Pending) == true )
                return new BadRequestObjectResult(new ApiResponse(400, "You already have a pending request to this team."));



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

                var response = new { requestId = joinRequest.Id, teamId = team.Id, message = "Join request sent successfully." };


                return new OkObjectResult(response);
               

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing join request: {ex.Message}");
                return new ObjectResult("An error occurred while processing the request") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        } 
        #endregion

        #region RespondJoinRequest
        public async Task<ActionResult> RespondToJoinRequestAsync(ClaimsPrincipal user, int requestId, string decision)
        {
            // get user 
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Include(s => s.Team)
                    .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team == null )
                return new ObjectResult(new ApiResponse(403, "You are not part of a team.")) { StatusCode = StatusCodes.Status403Forbidden };


            var request = await _unitOfWork.GetRepository<TeamJoinRequest>()
                .GetAllAsync()
                .Include(r => r.Team)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.TeamId != student.Team.Id)
                return new NotFoundObjectResult(new ApiResponse(404, "Request not found or not associated with your team."));
           
            if (request.Status != JoinRequestStatus.Pending)
                return new BadRequestObjectResult(new ApiResponse(400, "Request has already been responded to."));

            // Validate Decision to be either Accepted or Rejected only
            if ( !Enum.TryParse<JoinRequestStatus>(decision, true, out var requestStatus) ||
                ( requestStatus != JoinRequestStatus.Accepted && requestStatus != JoinRequestStatus.Rejected ) )
                return new BadRequestObjectResult(new ApiResponse(400, "Decision must be 'Accepted' or 'Rejected'."));

            string title = "";
            string content = "";
            Notification notification = null;

            try
            {
                if (decision.ToLower() == "accepted" )
                {
                    if (request.Team.TeamMembers?.Count >= request.Team.MaxMembers)
                        return new BadRequestObjectResult(new ApiResponse(400, "Team has reached its maximum member limit."));
                    
                    request.Status = JoinRequestStatus.Accepted;
                    request.Team.TeamMembers.Add(request.Student);

                    // Update team status if full
                    if ( request.Team.TeamMembers.Count >= request.Team.MaxMembers )
                        request.Team.IsOpenToJoin = false;


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
                else if (decision.ToLower() == "rejected" )
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

            var response = new
            {
                requestId = request.Id,
                teamId = request.TeamId,
                status = request.Status.ToString(),
                message = $"Request has been {request.Status.ToString().ToLower()}."
            };

            return new OkObjectResult(response);


        }
        #endregion

        #region GetTeamJoinRequests
        public async Task<ActionResult> GetTeamJoinRequestsAsync(ClaimsPrincipal user, int teamId)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team?.Id != teamId )
                return new ObjectResult(new ApiResponse(403, "You are not part of this team.")) { StatusCode = StatusCodes.Status403Forbidden };


            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.JoinRequests)
                    .ThenInclude(r => r.Student)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if ( team == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Team not found."));


            var requests = team.JoinRequests?.Select(r => new TeamJoinRequestResponseDto
            {
                RequestId = r.Id,
                StudentId = r.StudentId,
                StudentName = r.Student.User.UserName,
                Message = r.Message,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList() ?? new List<TeamJoinRequestResponseDto>();

            return new OkObjectResult(requests);
        }
        #endregion

        #region RequestSupervisor
        public async Task<IActionResult> RequestSupervisorAsync(ClaimsPrincipal user, SendProjectIdeaRequestDto dto)
        {
            // get user
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));


            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team == null )
                return new ObjectResult(new ApiResponse(403, "You are not a member of a team.")) { StatusCode = StatusCodes.Status403Forbidden };


            var idea = await _unitOfWork.GetRepository<ProjectIdea>()
                .GetAllAsync()
                .FirstOrDefaultAsync(i => i.Id == dto.ProjectIdeaId && i.TeamId == student.Team.Id);

            if ( idea == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Project idea not found or not associated with your team."));
           
            if ( idea.Status != ProjectIdeaStatus.Pending )
                return new BadRequestObjectResult(new ApiResponse(400, "Project idea must be in Pending status to request a supervisor."));
            
            if ( idea.SupervisorId != null )
                return new BadRequestObjectResult(new ApiResponse(400, "Project idea already has a supervisor."));

            // Check for existing pending request
            var existingRequest = await _unitOfWork.GetRepository<ProjectIdeaRequest>()
                .GetAllAsync()
                .AnyAsync(r => r.ProjectIdeaId == dto.ProjectIdeaId && r.Status == ProjectIdeaStatus.Pending);
            if ( existingRequest )
                return new BadRequestObjectResult(new ApiResponse(400, "A pending supervisor request already exists for this project idea."));



            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                  .GetAllAsync()
                  .Include(s => s.User)
                  .FirstOrDefaultAsync(s => s.Id == dto.SupervisorId);


            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found."));


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

                var response = new
                {
                    requestId = request.Id,
                    projectIdeaId = idea.Id,
                    supervisorId = supervisor.Id,
                    message = "Request sent to supervisor."
                };

                return new OkObjectResult(response);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing notification: {ex.Message}");
                return new ObjectResult("An error occurred while processing the request") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
        #endregion

    }
}
