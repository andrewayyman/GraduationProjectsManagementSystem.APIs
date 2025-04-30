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
using System.Threading.Tasks;
using System.Globalization;

namespace Graduation_Project_Management.Service
{
    public class TasksServices : ITasksServices
    {
        #region Dependencies

        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Task> _tasksRepo;
        private readonly IUnitOfWork _unitOfWork;

        public TasksServices( ApplicationDbContext context, IGenericRepository<Task> tasksRepo, IUnitOfWork unitOfWork )
        {
            _context = context;
            _tasksRepo = tasksRepo;
            _unitOfWork = unitOfWork;
        }

        #endregion Dependencies

        #region GetAllTasks Service

        public async Task<ActionResult> GetAllTasksAsync()
        {
            var tasks = await _tasksRepo.GetAllAsync()
           .Include(t => t.Team)
           .Include(t => t.Supervisor)
           .Include(t => t.AssignedStudent)
           .ToListAsync();

            if ( tasks == null || !tasks.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, "No tasks found."));

            var response = tasks.Select(async t =>
            {
                // Get the project idea by team id
                var projectIdea = await _context.ProjectIdeas
                    .FirstOrDefaultAsync(p => p.TeamId == t.TeamId);

                return new TaskResponseDto
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.Status.ToString(),
                    TeamName = t.Team?.Name,
                    SupervisorName = t.Supervisor != null ? $"{t.Supervisor.FirstName} {t.Supervisor.LastName}" : null,
                    AssignedStudentName = t.AssignedStudent != null ? $"{t.AssignedStudent.FirstName} {t.AssignedStudent.LastName}" : null,
                    ProjectIdeaTitle = projectIdea?.Title,
                    Message = "Task Retrieved Successfully"
                };
            }).Select(t => t.Result).ToList();

            return new OkObjectResult(response);
        }

        #endregion GetAllTasks Service

        #region CreateTask Service

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

            await _tasksRepo.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            var assignedStudent = task.AssignedStudent;
            var teamName = team.Name;
            var supervisorName = $"{supervisor.FirstName} {supervisor.LastName}";
            var projectIdeaTitle = projectIdea.Title;

            var response = new
            {
                task.Id,
                task.Title,
                task.Description,
                task.Deadline,
                Status = task.Status.ToString(),
                Team = new { team.Id, Name = teamName },
                Supervisor = new { supervisor.Id, Name = supervisorName },
                AssignedStudent = assignedStudent != null ? new { assignedStudent.Id, Name = $"{assignedStudent.FirstName} {assignedStudent.LastName}" } : null,
                ProjectIdea = projectIdea != null ? new { projectIdea.Id, Title = projectIdeaTitle } : null,
                Message = "Task Created Successfully"
            };

            return new OkObjectResult(response);
        }

        #endregion CreateTask Service

        #region UpdateTask Service

        public async Task<ActionResult> UpdateTaskAsync( int taskId, UpdateTaskDto dto, ClaimsPrincipal user )
        {
            // get supervisor from token
            var email = user.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            // check supervisor exist
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

            // check task exists and belongs to supervisor
            var task = await _context.Tasks
                                     .Include(t => t.Team)
                                     .ThenInclude(t => t.TeamMembers)
                                     .FirstOrDefaultAsync(t => t.Id == taskId && t.SupervisorId == supervisor.Id);

            // validate taskId
            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found or does not belong to the current supervisor."));

            // Validate assigned student is in team
            var isStudentInTeam = task.Team.TeamMembers.Any(m => m.Id == dto.AssignedToId);
            if ( !isStudentInTeam )
                return new BadRequestObjectResult(new ApiResponse(400, "Assigned student not found or is not in the team."));

            // update
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.AssignedStudentId = dto.AssignedToId;
            task.Deadline = dto.Deadline;
            task.Status = dto.Status;

            await _context.SaveChangesAsync();
            return new OkObjectResult(new ApiResponse(200, "Task updated successfully."));
        }

        #endregion UpdateTask Service

        #region DeleteTask Service

        public async Task<ActionResult> DeleteTaskAsync( int taskId, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);

            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

            var task = await _context.Tasks
                                     .Include(t => t.Team)
                                     .ThenInclude(t => t.TeamMembers)
                                     .FirstOrDefaultAsync(t => t.Id == taskId && t.SupervisorId == supervisor.Id);

            if ( task is null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found or does not belong to the current supervisor."));

            await _tasksRepo.DeleteAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Task deleted successfully."));
        }

        #endregion DeleteTask Service

        #region GetTaskByID Service

        public async Task<ActionResult> GetTaskByIdAsync( int taskId )
        {
            var task = await _context.Tasks
         .Include(t => t.Team)
         .Include(t => t.Supervisor)
         .Include(t => t.AssignedStudent)
         .FirstOrDefaultAsync(t => t.Id == taskId);

            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found."));

            // Get the project idea by team id
            var projectIdea = await _context.ProjectIdeas
                .FirstOrDefaultAsync(p => p.TeamId == task.TeamId);

            var response = new
            {
                task.Id,
                task.Title,
                task.Description,
                task.Deadline,
                Status = task.Status.ToString(),
                Team = new { task.Team.Id, Name = task.Team.Name },
                Supervisor = new { task.Supervisor.Id, Name = $"{task.Supervisor.FirstName} {task.Supervisor.LastName}" },
                AssignedStudent = task.AssignedStudent != null
                    ? new { task.AssignedStudent.Id, Name = $"{task.AssignedStudent.FirstName} {task.AssignedStudent.LastName}" }
                    : null,
                ProjectIdea = projectIdea != null
                    ? new { projectIdea.Id, Title = projectIdea.Title }
                    : null,
                Message = "Task Retrieved Successfully"
            };

            return new OkObjectResult(response);
        }

        #endregion GetTaskByID Service

        #region GetTaskByTeamId Service

        public async Task<ActionResult> GetTasksByTeamIdAsync( int teamId )
        {
            var tasks = await _tasksRepo.GetAllAsync()
                .Include(t => t.Team)
                .Include(t => t.Supervisor)
                .Include(t => t.AssignedStudent)
                .Where(t => t.TeamId == teamId)
                .ToListAsync();

            if ( tasks == null || !tasks.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, $"No tasks found for team ID {teamId}."));

            var response = tasks.Select(async t =>
            {
                var projectIdea = await _context.ProjectIdeas
                    .FirstOrDefaultAsync(p => p.TeamId == t.TeamId);

                return new TaskResponseDto
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.Status.ToString(),
                    TeamName = t.Team?.Name,
                    SupervisorName = t.Supervisor != null ? $"{t.Supervisor.FirstName} {t.Supervisor.LastName}" : null,
                    AssignedStudentName = t.AssignedStudent != null ? $"{t.AssignedStudent.FirstName} {t.AssignedStudent.LastName}" : null,
                    ProjectIdeaTitle = projectIdea?.Title,
                    Message = "Task Retrieved Successfully"
                };
            }).Select(t => t.Result).ToList();

            return new OkObjectResult(response);
        }

        #endregion GetTaskByTeamId Service

        #region GetTasksBySuperId Service

        public async Task<ActionResult> GetTasksBySupervisorIdAsync( int supervisorId )
        {
            var tasks = await _tasksRepo.GetAllAsync()
        .Include(t => t.Team)
        .Include(t => t.Supervisor)
        .Include(t => t.AssignedStudent)
        .Where(t => t.SupervisorId == supervisorId)
        .ToListAsync();

            if ( tasks == null || !tasks.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, $"No tasks found for supervisor ID {supervisorId}."));

            var response = tasks.Select(async t =>
            {
                var projectIdea = await _context.ProjectIdeas
                    .FirstOrDefaultAsync(p => p.TeamId == t.TeamId);

                return new TaskResponseDto
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.Status.ToString(),
                    TeamName = t.Team?.Name,
                    SupervisorName = t.Supervisor != null ? $"{t.Supervisor.FirstName} {t.Supervisor.LastName}" : null,
                    AssignedStudentName = t.AssignedStudent != null ? $"{t.AssignedStudent.FirstName} {t.AssignedStudent.LastName}" : null,
                    ProjectIdeaTitle = projectIdea?.Title,
                    Message = "Task Retrieved Successfully"
                };
            }).Select(t => t.Result).ToList();

            return new OkObjectResult(response);
        }

        #endregion GetTasksBySuperId Service

        #region GetTasksByStudedntId Service

        public async Task<ActionResult> GetTasksByStudentIdAsync( int assignedToId )
        {
            var tasks = await _tasksRepo.GetAllAsync()
        .Include(t => t.Team)
        .Include(t => t.Supervisor)
        .Include(t => t.AssignedStudent)
        .Where(t => t.AssignedStudentId == assignedToId)
        .ToListAsync();

            if ( tasks == null || !tasks.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, $"No tasks found for student ID {assignedToId}."));

            var response = tasks.Select(async t =>
            {
                var projectIdea = await _context.ProjectIdeas
                    .FirstOrDefaultAsync(p => p.TeamId == t.TeamId);

                return new TaskResponseDto
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.Status.ToString(),
                    TeamName = t.Team?.Name,
                    SupervisorName = t.Supervisor != null ? $"{t.Supervisor.FirstName} {t.Supervisor.LastName}" : null,
                    AssignedStudentName = t.AssignedStudent != null ? $"{t.AssignedStudent.FirstName} {t.AssignedStudent.LastName}" : null,
                    ProjectIdeaTitle = projectIdea?.Title,
                    Message = "Task Retrieved Successfully"
                };
            }).Select(t => t.Result).ToList();

            return new OkObjectResult(response);
        }

        #endregion GetTasksByStudedntId Service

        // msh fahm feha 7aga bs sha8alaa , date only not accurate

        #region FilterTasks Service

        public async Task<ActionResult> FilterTasksAsync( TaskFilterDto filter )
        {
            var query = _tasksRepo.GetAllAsync()
                .Include(t => t.Team)
                .Include(t => t.Supervisor)
                .Include(t => t.AssignedStudent)
                .GroupJoin(_context.ProjectIdeas,
                    task => task.TeamId,
                    project => project.TeamId,
                    ( task, projects ) => new { Task = task, Projects = projects })
                .SelectMany(
                    x => x.Projects.DefaultIfEmpty(),
                    ( x, project ) => new { x.Task, Project = project })
                .AsQueryable();

            // Apply filters
            if ( filter.TeamId.HasValue )
                query = query.Where(t => t.Task.TeamId == filter.TeamId.Value);

            if ( filter.SupervisorId.HasValue )
                query = query.Where(t => t.Task.SupervisorId == filter.SupervisorId.Value);

            if ( filter.AssignedStudentId.HasValue )
                query = query.Where(t => t.Task.AssignedStudentId == filter.AssignedStudentId.Value);

            // Status filter
            if ( !string.IsNullOrEmpty(filter.Status) )
            {
                if ( !Enum.TryParse<TaskStatusEnum>(filter.Status, true, out var status) )
                    return new BadRequestObjectResult(new ApiResponse(400, $"Invalid task status: {filter.Status}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(TaskStatusEnum)))}"));
                query = query.Where(t => t.Task.Status == status);
            }

            // Deadline filters
            if ( !string.IsNullOrEmpty(filter.DeadlineFrom) )
            {
                if ( !DateTime.TryParseExact(filter.DeadlineFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadlineFrom) )
                    return new BadRequestObjectResult(new ApiResponse(400, $"Invalid DeadlineFrom format: {filter.DeadlineFrom}. Use DD-MM-YYYY."));
                query = query.Where(t => t.Task.Deadline >= deadlineFrom);
            }

            if ( !string.IsNullOrEmpty(filter.DeadlineTo) )
            {
                if ( !DateTime.TryParseExact(filter.DeadlineTo, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadlineTo) )
                    return new BadRequestObjectResult(new ApiResponse(400, $"Invalid DeadlineTo format: {filter.DeadlineTo}. Use DD-MM-YYYY."));
                query = query.Where(t => t.Task.Deadline <= deadlineTo);
            }

            var tasks = await query.ToListAsync();

            if ( tasks == null || !tasks.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, "No tasks found matching the criteria."));

            var response = tasks.Select(t => new TaskResponseDto
            {
                TaskId = t.Task.Id,
                Title = t.Task.Title,
                Description = t.Task.Description,
                Deadline = t.Task.Deadline,
                Status = t.Task.Status.ToString(),
                TeamName = t.Task.Team?.Name,
                SupervisorName = t.Task.Supervisor != null ? $"{t.Task.Supervisor.FirstName} {t.Task.Supervisor.LastName}" : null,
                AssignedStudentName = t.Task.AssignedStudent != null ? $"{t.Task.AssignedStudent.FirstName} {t.Task.AssignedStudent.LastName}" : null,
                ProjectIdeaTitle = t.Project?.Title,
                Message = "Task Retrieved Successfully"
            }).ToList();

            return new OkObjectResult(response);
        }

        #endregion FilterTasks Service

        #region ChangeTaskStatus Service

        public async Task<ActionResult> ChangeTaskStatusAsync( int taskId, TaskStatusEnum newStatus, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var stundet = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);

            if ( stundet == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student not found from token."));

            // get task and verify it belongs to the student
            var task = await _context.Tasks
                                     .Include(t => t.Team)
                                     .ThenInclude(t => t.TeamMembers)
                                     .Include(t => t.Supervisor)
                                     .Include(t => t.AssignedStudent)
                                     .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedStudentId == stundet.Id);
            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found or not assigned to this student."));

            // cannot move to completed unless InProgress
            if ( newStatus == TaskStatusEnum.Completed && task.Status != TaskStatusEnum.InProgress )
                return new BadRequestObjectResult(new ApiResponse(400, "Task must be InProgress before marking as Completed."));

            // Update task status
            task.Status = newStatus;

            await _unitOfWork.SaveChangesAsync();

            var projectIdea = await _context.ProjectIdeas
               .FirstOrDefaultAsync(p => p.TeamId == task.TeamId);

            var response = new TaskResponseDto
            {
                TaskId = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                TeamName = task.Team?.Name,
                SupervisorName = task.Supervisor != null ? $"{task.Supervisor.FirstName} {task.Supervisor.LastName}" : null,
                AssignedStudentName = task.AssignedStudent != null ? $"{task.AssignedStudent.FirstName} {task.AssignedStudent.LastName}" : null,
                ProjectIdeaTitle = projectIdea?.Title,
                Message = "Task status updated successfully"
            };

            return new OkObjectResult(response);
        }

        #endregion ChangeTaskStatus Service
    }
}