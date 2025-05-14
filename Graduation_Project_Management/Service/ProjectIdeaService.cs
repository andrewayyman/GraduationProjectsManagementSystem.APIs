using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
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

        public ProjectIdeaService(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public async Task<IActionResult> PublishProjectIdeaAsync(ClaimsPrincipal user, PublishProjectIdeaDto dto)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _context.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student?.Team == null)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

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
            return new OkObjectResult(new { message = "Idea published successfully", ideaId = idea.Id });
        }
        public async Task<IActionResult> DeleteIdeaAsync(ClaimsPrincipal user, int ideaId)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var student = await _context.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.Email == email);
            if (student?.Team == null) return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var idea = await _context.ProjectIdeas.FirstOrDefaultAsync(i => i.Id == ideaId && i.TeamId == student.Team.Id);
            if (idea == null) return new NotFoundObjectResult("Idea not found or you don't have permission");

            await _unitOfWork.GetRepository<ProjectIdea>().DeleteAsync(idea);
            await _unitOfWork.SaveChangesAsync();
            return new OkObjectResult(new { message = "Idea deleted successfully" });
        }

        public async Task<IActionResult> UpdateIdeaAsync(ClaimsPrincipal user, int ideaId, PublishProjectIdeaDto dto)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var student = await _context.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.Email == email);
            if (student?.Team == null) return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var idea = await _context.ProjectIdeas.FirstOrDefaultAsync(i => i.Id == ideaId && i.TeamId == student.Team.Id);
            if (idea == null) return new NotFoundObjectResult("Idea not found");
            if (idea.Status == ProjectIdeaStatus.Accepted) return new BadRequestObjectResult("Idea already accepted");

            idea.Title = dto.Title ?? idea.Title;
            idea.Description = dto.Description ?? idea.Description;
            idea.TechStack = dto.TechStack ?? idea.TechStack;
            idea.Status = ProjectIdeaStatus.Pending;

            await _unitOfWork.GetRepository<ProjectIdea>().UpdateAsync(idea);
            await _unitOfWork.SaveChangesAsync();
            return new OkObjectResult(new { message = "Idea updated successfully" });
        }
    }
}
