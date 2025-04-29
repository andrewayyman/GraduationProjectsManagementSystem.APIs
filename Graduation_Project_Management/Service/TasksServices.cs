using Domain.Repository;
using Graduation_Project_Management.DTOs.TasksDTOs;
using Graduation_Project_Management.IServices;
using Repository;
using Repository.Identity;
using Domain.Entities;
using Task = Domain.Entities.Task;
using Domain.Enums;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Graduation_Project_Management.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_Management.Service
{
    public class TasksServices : ITasksServices
    {
        #region Dependencies

        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Task> _supervisorRepo;
        private readonly IUnitOfWork _unitOfWork;

        public TasksServices( ApplicationDbContext context, IGenericRepository<Task> supervisorRepo, IUnitOfWork unitOfWork )
        {
            _context = context;
            _supervisorRepo = supervisorRepo;
            _unitOfWork = unitOfWork;
        }

        #endregion Dependencies

        #region CreateTask

        public async Task<ActionResult> CreateTaskAsync( CreateTaskDto dto, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            // check supervisor exist
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

            // check team exists and belongs to supervisor
            var team = await _context.Teams
                                     .Include(t => t.TeamMembers)
                                     .FirstOrDefaultAsync(t => t.Id == dto.TeamId && t.SupervisorId == supervisor.Id);
            if ( team == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Invalid team ID or team does not belong to the current supervisor."));

            // check if student in this team
            var isStudentInTeam = team.TeamMembers.Any(m => m.Id == dto.AssignedToId);
            if ( !isStudentInTeam )
                return new NotFoundObjectResult(new ApiResponse(404, "Assigned student is not a member of the selected team."));

            //check project exists and belongs to this team
            var projectIdea = await _context.ProjectIdeas.FirstOrDefaultAsync(p => p.Id == dto.ProjectIdeaId && p.TeamId == dto.TeamId);
            if ( projectIdea == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Project idea is invalid or doesn't belong to the selected team."));

            var task = new Task
            {
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                Status = TaskStatusEnum.Backlog,
                TeamId = dto.TeamId,
                SupervisorId = supervisor.Id,
                AssignedStudent = dto.AssignedToId != null ? await _context.Students.FirstOrDefaultAsync(s => s.Id == dto.AssignedToId) : null, // by this line if task
            };

            await _supervisorRepo.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            var response = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                TeamId = task.TeamId,
                SupervisorId = task.SupervisorId,
                AssignedStudentId = task.AssignedStudentId,
                ProjectIdeaId = dto.ProjectIdeaId,
                Message = "Task Createed Succefully"
            };

            return new OkObjectResult(response);
        }

        #endregion CreateTask
    }
}