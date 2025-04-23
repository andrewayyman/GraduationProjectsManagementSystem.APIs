using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        #region Dependencies

        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly ApplicationDbContext _context;

        public StudentsController( UserManager<AppUser> userManager, IGenericRepository<Student> studentRepo, ApplicationDbContext applicationDbContext )
        {
            _userManager = userManager;
            _studentRepo = studentRepo;
            _context = applicationDbContext;
        }

        #endregion Dependencies

        #region Create team

        [HttpPost("Create")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> CreateTeam( TeamDto model )
        {
            // Get logged in user ID from JWT
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Find the student linked to this user
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student == null )
                return NotFound("Student profile not found.");

            // Create new team
            var newTeam = new Team
            {
                Name = model.Name,
                Description = model.Description,
                TeamDepartment = model.TeamDepartment,
                IsOpenToJoin = model.IsOpenToJoin,
                MaxMembers = model.MaxMembers,
                TechStack = model.TechStack ?? new List<string>(),
                TeamMembers = new List<Student> { student } // Add the creator
            };

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();


            return Ok();
        }

        #endregion Create team

        #region Delete Team

        [HttpDelete("{teamId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam( int teamId )
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if ( team == null )
                return NotFound("Team not found");

            // هل المستخدم عضو في الفريق؟
            var isMember = team.TeamMembers.Any(m => m.Email == userEmail);
            if ( !isMember )
                return StatusCode(StatusCodes.Status403Forbidden, "You are not part of this team");
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return Ok("Team deleted successfully");
        }

        #endregion Delete Team

        #region Get All Available Teams

        [HttpGet("Available")]
        public async Task<IActionResult> GetAvailableTeams()
        {
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                .Where(t => t.TeamMembers.Count < 6)
                .ToListAsync();

            var result = teams.Select(t => new GetTeamsDto
            {
                Name = t.Name,
                Description = t.Description,
                TeamDepartment = t.TeamDepartment,
                TechStack = t.TechStack,
                MembersCount = t.TeamMembers.Count
            });

            return Ok(result);
        }

        #endregion Get All Available Teams

        #region Join Team

        [HttpPost("Join Team")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> RequestToJoinTeam( TeamJoinRequestDto model )
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if ( userEmail == null )
                return Unauthorized();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student == null )
                return NotFound("Student profile not found.");

            var team = await _context.Teams
                .Include(t => t.JoinRequests)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId);

            if ( team == null )
                return NotFound("Team not found.");

            // Check if student already in the team
            if ( team.TeamMembers?.Any(m => m.Id == student.Id) == true )
                return BadRequest("You are already a member of this team.");

            // Check if already requested
            if ( team.JoinRequests?.Any(r => r.StudentId == student.Id) == true )
                return BadRequest("You already have a pending request to this team.");

            // Create request
            var joinRequest = new TeamJoinRequest
            {
                StudentId = student.Id,
                TeamId = team.Id,
                Message = model.Message,
                Status = JoinRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.TeamJoinRequests.Add(joinRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Join request sent successfully." });
        }

        #endregion Join Team

        #region Get Join Requests

        [HttpGet("team/{teamId}/join-requests")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> GetTeamJoinRequests( int teamId )
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var student = await _context.Students
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student?.TeamId != teamId )
                return StatusCode(StatusCodes.Status403Forbidden, "You are not part of this team");

            var team = await _context.Teams
                .Include(t => t.JoinRequests)
                .ThenInclude(r => r.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if ( team == null )
                return NotFound("Team not Found");

            var requests = team.JoinRequests?.Select(r => new
            {
                r.Id,
                r.StudentId,
                StudentName = r.Student.User.UserName,
                r.Message,
                Status = Enum.GetName(typeof(JoinRequestStatus), r.Status),
                r.CreatedAt
            }).ToList();

            return Ok(requests);
        }

        #endregion Get Join Requests

        #region Respond to Join Requests

        [HttpPost("join-requests/{requestId}/respond")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> RespondToJoinRequest( int requestId, [FromQuery] string decision )
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var student = await _context.Students
                .Include(s => s.Team)
                .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student?.Team == null )
                return StatusCode(StatusCodes.Status403Forbidden, "You are not part of this team");

            var request = await _context.TeamJoinRequests
                .Include(r => r.Team)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if ( request == null || request.TeamId != student.Team.Id )
                return NotFound("Cannot Find The Request");

            if ( request.Status != JoinRequestStatus.Pending )
                return BadRequest("You Request Already have been responded ");

            if ( decision.ToLower() == "accept" )
            {
                if ( request.Team.TeamMembers?.Count >= request.Team.MaxMembers )
                    return BadRequest("Team is Full");

                request.Status = JoinRequestStatus.Accepted;
                request.Team.TeamMembers.Add(request.Student);
            }
            else if ( decision.ToLower() == "reject" )
            {
                request.Status = JoinRequestStatus.Rejected;
            }
            else
            {
                return BadRequest("The Request Must be Rejected Or Accepted");
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $" the request have been {request.Status} " });
        }

        #endregion Respond to Join Requests

        #region Update Student Profile

        [HttpPut("UpdateProfile")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentProfile( [FromBody] UpdateStudentProfileDto dto )
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);
            if ( student == null )
                return NotFound("Student not found");

            // التحديث
            student.FirstName = dto.FirstName ?? student.FirstName;
            student.LastName = dto.LastName ?? student.LastName;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Department = dto.Department ?? student.Department;
            student.Gpa = dto.Gpa ?? student.Gpa;
            student.TechStack = dto.TechStack ?? student.TechStack;
            student.GithubProfile = dto.GitHubProfile ?? student.GithubProfile;
            student.LinkedInProfile = dto.LinkedInProfile ?? student.LinkedInProfile;
            student.MainRole = dto.MainRole ?? student.MainRole;
            student.SecondaryRole = dto.SecondaryRole ?? student.SecondaryRole;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        #endregion Update Student Profile
    }
}