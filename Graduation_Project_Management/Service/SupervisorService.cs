using Domain.Entities.Identity;
using Domain.Entities;
using Domain.Repository;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.Errors;
using System.Security.Claims;
using Domain.Enums;

namespace Graduation_Project_Management.Service
{
    public class SupervisorService : ISupervisorService
    {
        #region Dependencies

        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Supervisor> _supervisorRepo;
        private readonly IUnitOfWork _unitOfWork;

        public SupervisorService( UserManager<AppUser> userManager, ApplicationDbContext context, IGenericRepository<Supervisor> supervisorRepo, IUnitOfWork unitOfWork )
        {
            _userManager = userManager;
            _context = context;
            _supervisorRepo = supervisorRepo;
            _unitOfWork = unitOfWork;
        }

        #endregion Dependencies

        #region GetAllSupervisors Service

        public async Task<ActionResult> GetAllSupervisorsAsync()
        {
            var supervisors = await _unitOfWork.GetRepository<Supervisor>().GetAllAsync().ToListAsync();
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
                        TechStack = t.TechStack
                    }).ToList(),
            };

            return new OkObjectResult(ReturnedSupervisor);
        }

        #endregion GetSupervisorById service

        #region UpdateSupervisor service

        public async Task<ActionResult> UpdateSupervisorProfileAsync( int id, UpdateSupervisorDto supervisorDto )
        {
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
                r.ProjectIdea.Title,
                r.ProjectIdea.Description,
                r.ProjectIdea.TechStack,
                r.ProjectIdea.Team?.Name,
                r.ProjectIdea.Team?.TeamMembers,
                r.ProjectIdea.Team?.TeamDepartment,
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

            // check if the supervisor exists
            if ( supervisor == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "Supervisor not found."));

            var teams = await _context.Teams
                .Where(t => t.SupervisorId == supervisor.Id)
                .ToListAsync();

            if ( teams == null || !teams.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, "There are no teams assigned to this supervisor."));

            var response = teams.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.TeamDepartment,
                t.IsOpenToJoin,
                t.MaxMembers,
                TechStack = string.Join(", ", t.TechStack ?? new List<string>()),
                TeamMembers = t.TeamMembers?.Select(tm => new { tm.Id, tm.FirstName, tm.LastName }),
                //CreatedAt = t.CreatedAt
            });
            return new OkObjectResult(response);
        }

        #endregion GetMyTeams Service

        #region HandleRequest Service

        public async Task<ActionResult> HandleIdeaRequestAsync( ClaimsPrincipal user, HandleIdeaRequestDto dto )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);
            Console.WriteLine("Extracted email from token: " + email);  // Or use a logger

            if ( supervisor == null )
                return new UnauthorizedObjectResult(new ApiResponse(401 , "Supervisor not found."));

            var request = await _context.ProjectIdeasRequest
                .Include(r => r.ProjectIdea)
                .ThenInclude(pi => pi.Team)

                .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.SupervisorId == supervisor.Id);

            if ( request == null )
                return new NotFoundObjectResult(new ApiResponse(404 , "Request not found"));

            if ( dto.IsApproved )
            {
                request.Status = ProjectIdeaStatus.Accepted;
                request.ProjectIdea.SupervisorId = supervisor.Id;
                request.ProjectIdea.Team.SupervisorId = supervisor.Id;
                request.ProjectIdea.Status = ProjectIdeaStatus.Accepted;
            }
            else
            {
                request.Status = ProjectIdeaStatus.Rejected;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = $"Request {( dto.IsApproved ? "approved" : "declined" )} successfully" });
        }

        #endregion HandleRequest Service
    }
}