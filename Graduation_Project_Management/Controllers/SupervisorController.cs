using Domain.Entities.Identity;
using Domain.Entities;
using Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repository.Identity;
using Microsoft.EntityFrameworkCore;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.Errors;
using System.Security.Claims;
using Domain.Enums;
using Graduation_Project_Management.DTOs;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Supervisor> _supervisorRepo;
        private readonly IUnitOfWork _unitOfWork;

        public SupervisorController( UserManager<AppUser> userManager, ApplicationDbContext applicationDbContext, IGenericRepository<Supervisor> supervisorRepo,IUnitOfWork unitOfWork )
        {
            _userManager = userManager;
            _context = applicationDbContext;
            _supervisorRepo = supervisorRepo;
            _unitOfWork = unitOfWork;
            
        }

        #region GetAllSupervisors

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupervisorDto>>> GetAll()
        {
            var supervisors = await _unitOfWork.GetRepository<Supervisor>().GetAllAsync().ToListAsync();


            // check if there are any supervisors
            if ( supervisors == null || !supervisors.Any() )
                return NotFound(new ApiResponse(404, "There is no any supervisors "));

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

            return Ok(ReturnedSupervisors);
        }

        #endregion GetAllSupervisors

        #region GetSupervisorById

        [HttpGet("{id}")]
        public async Task<ActionResult<SupervisorDto>> GetById( int id )
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);

            // if null
            if ( supervisor == null )
                return NotFound(new ApiResponse(404, "There is no any supervisors with this id "));

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

            return Ok(ReturnedSupervisor);
        }

        #endregion GetSupervisorById

        #region UpdateSupervisor

        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateSupervisorDto>> Update( int id, [FromBody] UpdateSupervisorDto supervisorDto )
        {
            // check if the supervisor exists
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            if ( supervisor == null )
                return NotFound(new ApiResponse(404, "There is no any supervisors with this id "));

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
            return Ok(new { message = "Profile Updated Successfully" });
        }

        #endregion UpdateSupervisor

        #region DeleteSupervisor

        [HttpDelete("{id}")]
        public async Task<ActionResult<SupervisorDto>> Delete( int id )
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);

            // check if the supervisor exists
            if ( supervisor == null )
                return NotFound(new ApiResponse(404, "There is no any supervisors with this id "));

            // delete the supervisor
            await _supervisorRepo.DeleteAsync(supervisor);
            return Ok(new { message = "Profile Deleted Successfully" });
        }

        #endregion DeleteSupervisor

        #region GetPendingRequests

        // need to be updated that once request approved remove it from here

        [HttpGet("GetPendingRequests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            if ( supervisor == null )
                return Unauthorized(new ApiResponse(401, "Supervisor not found."));

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

            return Ok(response);
        }

        #endregion GetPendingRequests

        #region GetMyTeams

        [HttpGet("GetMyTeams")]
        public async Task<IActionResult> GetMyTeams()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            // check if the supervisor exists
            if ( supervisor == null )
                return Unauthorized(new ApiResponse(401, "Supervisor not found."));

            var teams = await _context.Teams
                .Where(t => t.SupervisorId == supervisor.Id)
                .ToListAsync();

            if ( teams == null || !teams.Any() )
                return NotFound(new ApiResponse(404, "There are no teams assigned to this supervisor."));

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
            return Ok(response);
        }

        #endregion GetMyTeams

        #region HandleRequest

        [HttpPut("HandleRequest")]
        public async Task<IActionResult> HandleRequest( [FromBody] HandleIdeaRequestDto dto )
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);
            Console.WriteLine("Extracted email from token: " + email);  // Or use a logger

            if ( supervisor == null )
                return Unauthorized("Supervisor not found.");

            var request = await _context.ProjectIdeasRequest
                .Include(r => r.ProjectIdea)
                .ThenInclude(pi => pi.Team)

                .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.SupervisorId == supervisor.Id);

            if ( request == null )
                return NotFound("Request not found");

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

            return Ok(new { message = $"Request {( dto.IsApproved ? "approved" : "declined" )} successfully" });
        }

        #endregion HandleRequest
    }
}