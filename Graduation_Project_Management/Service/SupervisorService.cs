using Domain.Entities.Identity;
using Domain.Entities;
using Domain.Repository;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.Errors;
using System.Security.Claims;
using Domain.Enums;
using Graduation_Project_Management.Hubs;
using Microsoft.AspNetCore.SignalR;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;

namespace Graduation_Project_Management.Service
{
    public class SupervisorService : ISupervisorService
    {
        #region Dependencies

        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Supervisor> _supervisorRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _notificationHub;


        public SupervisorService( UserManager<AppUser> userManager, ApplicationDbContext context, IGenericRepository<Supervisor> supervisorRepo, IUnitOfWork unitOfWork,IHubContext<NotificationHub> notificationHub)
        {
            _userManager = userManager;
            _context = context;
            _supervisorRepo = supervisorRepo;
            _unitOfWork = unitOfWork;
            _notificationHub = notificationHub;
        }

        #endregion Dependencies

        #region GetAllSupervisors Service

        public async Task<ActionResult> GetAllSupervisorsAsync()
        {
            var supervisors = await _unitOfWork.GetRepository<Supervisor>()
                                               .GetAllAsync()
                                               .ToListAsync();

            if ( supervisors == null || !supervisors.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, "There are no supervisors."));

            var ReturnedSupervisors = supervisors.Select(s => new SupervisorDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Department = s.Department,
                ProfilePictureUrl = s.ProfilePictureUrl,
                MaxAssignedTeams = s.MaxAssignedTeams,
                PreferredTechnologies = s.PreferredTechnologies,
                SupervisedTeams = _context.Teams
                    .Where(t => t.SupervisorId == s.Id)
                    .Select(t => new TeamDto
                    {
                        Name = t.Name,
                        Description = t.Description,
                        TeamDepartment = t.TeamDepartment,
                        TechStack = t.TechStack
                    }).ToList(),
            }).ToList();

            return new OkObjectResult(ReturnedSupervisors);
        }

        #endregion GetAllSupervisors Service

        #region GetSupervisorById service

        public async Task<ActionResult> GetSupervisorByIdAsync( int id )
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);

            // if null
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "There is no any supervisors with this id "));

            var ReturnedSupervisor = new SupervisorDto()
            {
                Id = supervisor.Id,
                FirstName = supervisor.FirstName,
                LastName = supervisor.LastName,
                Email = supervisor.Email,
                PhoneNumber = supervisor.PhoneNumber,
                Department = supervisor.Department,
                ProfilePictureUrl = supervisor.ProfilePictureUrl,
                MaxAssignedTeams = supervisor.MaxAssignedTeams,
                PreferredTechnologies = supervisor.PreferredTechnologies,
                SupervisedTeams = _context.Teams
                    .Where(t => t.SupervisorId == supervisor.Id)
                    .Select(t => new TeamDto
                    {
                        Name = t.Name,
                        Description = t.Description,
                        TeamDepartment = t.TeamDepartment,
                        TechStack = t.TechStack
                    }).ToList(),
            };

            return new OkObjectResult(ReturnedSupervisor);
        }

        #endregion GetSupervisorById service

        #region GetSupervisorByEmail service
        public async Task<ActionResult> GetSupervisorByEmailAsync(string email)
        {
            var supervisor = await _context.Supervisors
                .Include(s => s.SupervisedTeams) 
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

            if ( string.IsNullOrEmpty(email) )
                return new BadRequestObjectResult(new ApiResponse(400, "Email is required."));

            if (supervisor == null)
                return new NotFoundObjectResult(new ApiResponse(404, "There is no supervisor with this email"));

            var ReturnedSupervisor = new SupervisorDto()
            {
                Id = supervisor.Id,
                FirstName = supervisor.FirstName,
                LastName = supervisor.LastName,
                Email = supervisor.Email,
                PhoneNumber = supervisor.PhoneNumber,
                Department = supervisor.Department,
                ProfilePictureUrl = supervisor.ProfilePictureUrl,
                MaxAssignedTeams = supervisor.MaxAssignedTeams,
                PreferredTechnologies = supervisor.PreferredTechnologies,
                SupervisedTeams = _context.Teams
                    .Where(t => t.SupervisorId == supervisor.Id)
                    .Select(t => new TeamDto
                    {
                        Name = t.Name,
                        Description = t.Description,
                        TeamDepartment = t.TeamDepartment,
                        TechStack = t.TechStack
                    }).ToList(),
            };

            return new OkObjectResult(ReturnedSupervisor);
        } 
        #endregion

        #region UpdateSupervisor service

        public async Task<ActionResult> UpdateSupervisorProfileAsync( int id, UpdateSupervisorDto supervisorDto )
        {
            if ( !string.IsNullOrEmpty(supervisorDto.Email) && await _unitOfWork.GetRepository<Supervisor>().GetAllAsync().AnyAsync(s => s.Email == supervisorDto.Email && s.Id != id) )
                return new BadRequestObjectResult(new ApiResponse(400, "Email is already in use."));

            // check if the supervisor exists
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "There is no any supervisors with this id "));


            // update the supervisor
            supervisor.FirstName = supervisorDto.FirstName ?? supervisor.FirstName;
            supervisor.LastName = supervisorDto.LastName ?? supervisor.LastName;
            supervisor.Email = supervisorDto.Email ?? supervisor.Email;
            supervisor.PhoneNumber = supervisorDto.PhoneNumber ?? supervisor.PhoneNumber;
            supervisor.Department = supervisorDto.Department ?? supervisor.Department;
            supervisor.ProfilePictureUrl = supervisorDto.ProfilePictureUrl ?? supervisor.ProfilePictureUrl;
            supervisor.PreferredTechnologies = supervisorDto.PreferredTechnologies ?? supervisor.PreferredTechnologies;
            supervisor.MaxAssignedTeams = supervisorDto.MaxAssignedTeams;

            await _supervisorRepo.UpdateAsync(supervisor);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new { message = "Profile Updated Successfully" });
        }

        #endregion UpdateSupervisor service

        #region DeleteSupervisor Service

        public async Task<ActionResult> DeleteSupervisorProfileAsync( int id )
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);

            // check if the supervisor exists
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "There is no any supervisors with this id "));

            // delete the supervisor
            await _supervisorRepo.DeleteAsync(supervisor);
            await _unitOfWork.SaveChangesAsync();
            return new OkObjectResult(new { message = "Profile Deleted Successfully" });
        
        
        }

        #endregion DeleteSupervisor Service

        #region GetPendingRequests Service

        public async Task<ActionResult> GetPendingRequestsAsync( ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            if ( supervisor == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "Supervisor not found."));

            var requests = await _context.ProjectIdeasRequest
                .Include(r => r.ProjectIdea)
                .Include(r => r.ProjectIdea.Team)
                .Where(r => r.SupervisorId == supervisor.Id && r.Status == ProjectIdeaStatus.Pending)
                .ToListAsync();

            var response = requests.Select(r => new
            {
                RequestId = r.Id,
                IdeaId = r.ProjectIdeaId,
                TeamId = r.ProjectIdea.TeamId,
                r.ProjectIdea.Team?.Name,
                r.ProjectIdea.Team?.TeamMembers,
                r.ProjectIdea.Team?.TeamDepartment,
                r.ProjectIdea.Title,
                r.ProjectIdea.Description,
                r.ProjectIdea.TechStack,
                Status = Enum.GetName(typeof(ProjectIdeaStatus), r.Status),
                r.CreatedAt
            });

            return new OkObjectResult(response);
        }

        #endregion GetPendingRequests Service

        #region GetMyTeams Service

        public async Task<ActionResult> GetMyTeamsAsync( ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            // Check if the supervisor exists
            if ( supervisor == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "Supervisor not found."));

            var teams = await _context.Teams
                .Where(t => t.SupervisorId == supervisor.Id)
                .Include(t => t.ProjectIdeas)
                .Include(t=>t.TeamMembers)
                .ToListAsync();

            if ( teams == null || !teams.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, "There are no teams assigned to this supervisor."));

            var response = teams.Select(t => new
            {
                t.Id,
                t.Name,
                ProjectIdeaTitle = t.ProjectIdeas?.FirstOrDefault(pi => pi.Status == ProjectIdeaStatus.Accepted)?.Title,
                t.Description,
                t.TeamDepartment,
                t.IsOpenToJoin,
                t.MaxMembers,
                TechStack = string.Join(", ", t.TechStack ?? new List<string>()),
                TeamMembers = t.TeamMembers?.Select(tm => new { tm.Id, FullName = $"{tm.FirstName} {tm.LastName}", })
            });

            return new OkObjectResult(response);
        }
        

        #endregion GetMyTeams Service

        #region HandleRequest Service

        public async Task<ActionResult> HandleIdeaRequestAsync(ClaimsPrincipal user, HandleIdeaRequestDto dto)
        {
            // get from token
            var email = user.FindFirstValue(ClaimTypes.Email);

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                    .GetAllAsync()
                    .Include(s => s.SupervisedTeams)
                    .FirstOrDefaultAsync(s => s.Email == email);

            if (supervisor == null)
                return new UnauthorizedObjectResult(new ApiResponse(401, "Supervisor not found."));

            var request = await _unitOfWork.GetRepository<ProjectIdeaRequest>()
                .GetAllAsync()
                .Include(r => r.ProjectIdea)
                    .ThenInclude(pi => pi.Team)
                        .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.SupervisorId == supervisor.Id);


            if ( request == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Request not found or not associated with this supervisor."));
            
            // make sure it's pending
            if ( request.Status != ProjectIdeaStatus.Pending )
                return new BadRequestObjectResult(new ApiResponse(400, "Request has already been processed."));

            // Validate Project Idea and Team
            if ( request.ProjectIdea.SupervisorId != null || request.ProjectIdea.Status != ProjectIdeaStatus.Pending )
                return new BadRequestObjectResult(new ApiResponse(400, "Project idea already has a supervisor or is not in Pending status."));

            if ( request.ProjectIdea.Team.SupervisorId != null )
                return new BadRequestObjectResult(new ApiResponse(400, "Team already has a supervisor."));

            // Validate Supervisor Capacity (for approval)
            if ( dto.IsApproved && supervisor.SupervisedTeams?.Count >= supervisor.MaxAssignedTeams )
                return new BadRequestObjectResult(new ApiResponse(400, "Supervisor has reached the maximum number of assigned teams."));


            try
            {
                string title = "";
                string content = "";
                Notification notification = null;

                if (dto.IsApproved )
                {
                    request.Status = ProjectIdeaStatus.Accepted;
                    request.ProjectIdea.SupervisorId = supervisor.Id;
                    request.ProjectIdea.Status = ProjectIdeaStatus.Accepted;
                    request.ProjectIdea.Team.SupervisorId = supervisor.Id;

                    // Reject other pending requests for the same project idea
                    var otherRequests = await _unitOfWork.GetRepository<ProjectIdeaRequest>()
                        .GetAllAsync()
                        .Where(r => r.ProjectIdeaId == request.ProjectIdeaId && r.Id != request.Id && r.Status == ProjectIdeaStatus.Pending)
                        .ToListAsync();

                    foreach ( var otherRequest in otherRequests )
                    {
                        otherRequest.Status = ProjectIdeaStatus.Rejected;
                    }

                    title = "Project Idea Request Approved";
                    content = $"Your project idea request for team '{request.ProjectIdea.Team.Name}' has been approved by the supervisor.";
                }
                else
                {
                    request.Status = ProjectIdeaStatus.Rejected;

                    title = "Project Idea Request Rejected";
                    content = $"Your project idea '{request.ProjectIdea.Title}' for team '{request.ProjectIdea.Team.Name}' has been rejected by the supervisor. Reason: {dto.RejectionReason ?? "Not specified."}";
                }

                // Save changes to update request and project idea
                await _context.SaveChangesAsync();
                Console.WriteLine("Project idea request changes saved to database.");

                // Send notifications to team members
                foreach (var member in request.ProjectIdea.Team.TeamMembers)
                {
                    notification = new Notification
                    {
                        Message = content,
                        RecipientId = member.UserId,
                        Type = NotificationType.ProjectIdeaRequest,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Log notification details
                    Console.WriteLine($"Preparing notification: RecipientId={member.UserId}, Title={title}, Content={content}");

                    // Save notification to database
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
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

                var response = new
                {
                    message = $"Request {( dto.IsApproved ? "approved" : "declined" )} successfully" ,
                    requestId = request.Id,
                    projectIdeaId = request.ProjectIdeaId,
                    status = request.Status.ToString()
                };

                return new OkObjectResult(response);
            
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing project idea request: {ex.Message}");
                return new ObjectResult(new ApiResponse(500, "An error occurred while processing the request"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };



            }
        }
        #endregion HandleRequest Service

       



    }
}
