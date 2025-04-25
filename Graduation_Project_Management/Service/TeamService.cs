using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Service
{
    public class TeamService : ITeamService
    {
        #region Dependencies
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Create
        public async Task<ActionResult> CreateTeamAsync(ClaimsPrincipal user, TeamDto model)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student == null)
                return new NotFoundObjectResult("Student profile not found.");

            if (student.Team != null)
                return new BadRequestObjectResult("You are already part of a team.");

            var team = new Team
            {
                Name = model.Name,
                Description = model.Description,
                TeamDepartment = model.TeamDepartment,
                TechStack = model.TechStack ?? new List<string>(),
                TeamMembers = new List<Student> { student },
                IsOpenToJoin = true
            };

            await _unitOfWork.GetRepository<Team>().AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new OkResult();
        }
        #endregion

        #region Delete 
        public async Task<IActionResult> DeleteTeamAsync(ClaimsPrincipal user, int teamId)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return new NotFoundObjectResult("Team not found");

            if (!team.TeamMembers.Any(m => m.Email == userEmail))
                return new ObjectResult("You are not part of this team") { StatusCode = StatusCodes.Status403Forbidden };

            await _unitOfWork.GetRepository<Team>().DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult("Team deleted successfully");
        }
        #endregion

        #region Get All
        public async Task<IActionResult> GetAvailableTeamsAsync()
        {
            var teams = await _unitOfWork.GetRepository<Team>().GetAllAsync()
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

            return new OkObjectResult(result);
        } 
        #endregion

    }
}

