using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.Errors;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Service
{
    public class ProjectIdeaService :IProjectIdeaService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ProjectIdeaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        #region PublishProjectIDea
        public async Task<IActionResult> PublishProjectIdeaAsync( ClaimsPrincipal user, PublishProjectIdeaDto dto )
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Where(s => s.Email == userEmail)
                .Include(s => s.Team)
                .FirstOrDefaultAsync();



            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team == null )
                return new BadRequestObjectResult(new ApiResponse(400, "You are not a member of any team."));


            var idea = new ProjectIdea
            {
                Title = dto.Title,
                Description = dto.Description,
                TechStack = dto.TechStack,
                TeamId = student.Team.Id,
                CreatedAt = DateTime.UtcNow,
                Status = ProjectIdeaStatus.Pending
            };


            await _unitOfWork.GetRepository<ProjectIdea>().AddAsync(idea);
            await _unitOfWork.SaveChangesAsync();

            var response = new
            {
                projectIdeaId = idea.Id,
                apiResponse = new ApiResponse(200, "Project idea published successfully.")
            };

            return new OkObjectResult(response);


        }
        #endregion

        #region GetAllTeamIdeasByStudentId

        public async Task<IActionResult> GetAllTeamIdeasByStudentIdAsync (ClaimsPrincipal user , int studentId)
        {
            // get user from token
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // Get Student
            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Where(s=>s.Id == studentId)
                .Include(s => s.Team)
                    .ThenInclude(t=>t.ProjectIdeas)
                        .ThenInclude(p=>p.Supervisor)
                .FirstOrDefaultAsync();


            // Validate
            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));

            if ( student.Team == null )
                return new BadRequestObjectResult(new ApiResponse(400, "You are not a member of any team."));

            if ( student.Email != userEmail )
                return new ObjectResult(new ApiResponse(403, "You are not authorized to view this student's team ideas."));

            // GetIdeas 
            var ideas = student.Team.ProjectIdeas;
            var result = ideas.Select(i => new ProjectIdeaDto
            {
                ProjectIdeaId = i.Id,
                Title = i.Title,
                Description = i.Description,
                TechStack = i.TechStack,
                CreatedAt = i.CreatedAt.ToString("yyyy-MM-dd"),
                Status = i.Status.ToString(),
                Supervisor = i.Supervisor != null ? new ShortSupervisorDto
                {
                    SupervisorId = i.Supervisor.Id,
                    Name = i.Supervisor.FirstName + " " + i.Supervisor.LastName
                } : null
            }).ToList();


         

            return new OkObjectResult(result);
        }


        



        #endregion

        #region UpdateProjectIdea
        public async Task<IActionResult> UpdateIdeaAsync( ClaimsPrincipal user, int ideaId, PublishProjectIdeaDto dto )
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                            .GetAllAsync()
                            .Where(s => s.Email == userEmail)
                            .Include(s => s.Team)
                            .FirstOrDefaultAsync();

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team == null )
                return new BadRequestObjectResult(new ApiResponse(400, "You are not a member of any team."));


            var idea = await _unitOfWork.GetRepository<ProjectIdea>()
                            .GetAllAsync()
                            .Where(i => i.Id == ideaId && i.TeamId == student.Team.Id)
                            .FirstOrDefaultAsync();

            if ( idea == null )
                return new NotFoundObjectResult(new ApiResponse(404, $"Project idea with ID {ideaId} not found or not owned by your team."));

            if ( idea.Status == ProjectIdeaStatus.Accepted || idea.Status == ProjectIdeaStatus.Rejected )
                return new BadRequestObjectResult(new ApiResponse(400, "Cannot update an accepted or rejected idea."));


            idea.Title = dto.Title ?? idea.Title;
            idea.Description = dto.Description ?? idea.Description;
            if ( dto.TechStack != null && dto.TechStack.Any() )
                idea.TechStack = dto.TechStack;

            idea.Status = ProjectIdeaStatus.Pending;
            idea.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<ProjectIdea>().UpdateAsync(idea);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Project idea updated successfully."));
        }
        #endregion

        #region DeleteProjectIdea
        public async Task<IActionResult> DeleteIdeaAsync( ClaimsPrincipal user, int ideaId )
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(userEmail) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Where(s => s.Email == userEmail)
                .Include(s => s.Team)
                .FirstOrDefaultAsync();

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));
            if ( student.Team == null )
                return new BadRequestObjectResult(new ApiResponse(400, "You are not a member of any team."));


            var idea = await _unitOfWork.GetRepository<ProjectIdea>()
                  .GetAllAsync()
                  .Where(i => i.Id == ideaId && i.TeamId == student.Team.Id)
                  .FirstOrDefaultAsync();

            if ( idea == null )
                return new NotFoundObjectResult(new ApiResponse(404, $"Project idea with ID {ideaId} not found or not owned by your team."));

            if ( idea.Status == ProjectIdeaStatus.Accepted )
                return new BadRequestObjectResult(new ApiResponse(400, "Cannot delete an accepted idea."));


            await _unitOfWork.GetRepository<ProjectIdea>().DeleteAsync(idea);
            await _unitOfWork.SaveChangesAsync();
            return new OkObjectResult(new ApiResponse(200, "Project idea deleted successfully."));
        }
        #endregion




    }
}
