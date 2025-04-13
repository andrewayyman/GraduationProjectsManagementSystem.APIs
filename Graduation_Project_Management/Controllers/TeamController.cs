using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
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
    public class TeamController : ControllerBase

    {
        #region MyRegion Dependencies

        private readonly ApplicationDbContext _context;

        public TeamController( ApplicationDbContext applicationDbContext )
        {
            _context = applicationDbContext;
        }

        #endregion MyRegion Dependencies

        #region Publish Project Idea

        [HttpPost("PublishIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> PublishProjectIdea( [FromBody] PublishProjectIdeaDto dto )
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var student = await _context.Students
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student == null || student.Team == null )
                return StatusCode(StatusCodes.Status403Forbidden, "You must be part of a team to publish a project idea.");

            var idea = new ProjectIdea
            {
                Title = dto.Title,
                Description = dto.Description,
                TechStack = dto.TechStack,
                TeamId = student.Team.Id,
                CreatedAt = DateTime.UtcNow,
                Status = ProjectIdeaStatus.Pending
                // SupervisorId is not assigned yet
            };

            _context.ProjectIdeas.Add(idea);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Idea published successfully", ideaId = idea.Id });
        }

        #endregion Publish Project Idea

        #region Send Idea Request To Supervisor

        [HttpPost("RequestSupervisor")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestSupervisorForIdea( [FromBody] SendProjectIdeaRequestDto dto )
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var student = await _context.Students
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if ( student?.Team == null )
                return StatusCode(StatusCodes.Status403Forbidden, "You must be in a team.");

            var idea = await _context.ProjectIdeas
                .FirstOrDefaultAsync(i => i.Id == dto.ProjectIdeaId && i.TeamId == student.Team.Id);

            if ( idea == null )
                return NotFound("Project idea not found");

            var supervisor = await _context.Supervisors.FindAsync(dto.SupervisorId);
            if ( supervisor == null )
                return NotFound("Supervisor not found");

            // أنشئ الريكوست
            var request = new ProjectIdeaRequest
            {
                ProjectIdeaId = idea.Id,
                SupervisorId = dto.SupervisorId,
                Status = ProjectIdeaStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectIdeasRequest.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request sent to supervisor" });
        }

        #endregion Send Idea Request To Supervisor
    }
}