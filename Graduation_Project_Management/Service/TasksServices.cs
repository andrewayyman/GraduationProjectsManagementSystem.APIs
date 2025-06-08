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

        public async Task<ActionResult> GetAllTasksAsync( ClaimsPrincipal user )
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail) )
                return new BadRequestObjectResult(new ApiResponse(400, "User email not found in token."));


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
            // Validate DTO
            if ( string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length < 5 || dto.Title.Length > 100 )
                return new BadRequestObjectResult(new ApiResponse(400, "Title must be between 5 and 100 characters."));
            if ( string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length < 20 || dto.Description.Length > 500 )
                return new BadRequestObjectResult(new ApiResponse(400, "Description must be between 20 and 500 characters."));
            if ( dto.Deadline <= DateTime.UtcNow )
                return new BadRequestObjectResult(new ApiResponse(400, "Deadline must be in the future."));

            // Validate User
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                 .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

           
            // Validate Team
            var team = await _unitOfWork.GetRepository<Team>()
                .GetAllAsync()
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId && t.SupervisorId == supervisor.Id);
            if ( team == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Invalid team ID or team does not belong to the current supervisor."));



            // Validate Assigned Student
            if ( dto.AssignedToId.HasValue && !team.TeamMembers.Any(m => m.Id == dto.AssignedToId) )
                return new NotFoundObjectResult(new ApiResponse(404, "Assigned student is not a member of the selected team."));


            // Validate Project Idea
            var projectIdea = await _unitOfWork.GetRepository<ProjectIdea>()
                .GetAllAsync()
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectIdeaId && p.TeamId == dto.TeamId);
            
            if ( projectIdea == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Project idea is invalid or doesn't belong to the selected team."));
            
            if ( projectIdea.Status != ProjectIdeaStatus.Accepted )
                return new BadRequestObjectResult(new ApiResponse(400, "Project idea must be in Accepted status to assign tasks."));


            // validate date
            if ( dto.Deadline <= DateTime.UtcNow )
                return new BadRequestObjectResult(new ApiResponse(400, "Deadline must be in the future."));


            var task = new Task
            {
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                Status = TaskStatusEnum.Backlog,
                TeamId = dto.TeamId,
                SupervisorId = supervisor.Id,
                AssignedStudentId = dto.AssignedToId

            };

            await _unitOfWork.GetRepository<Task>().AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Prepare Response
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
            // Validate DTO
            if ( string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length < 5 || dto.Title.Length > 100 )
                return new BadRequestObjectResult(new ApiResponse(400, "Title must be between 5 and 100 characters."));
            if ( string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length < 20 || dto.Description.Length > 500 )
                return new BadRequestObjectResult(new ApiResponse(400, "Description must be between 20 and 500 characters."));
            if ( dto.Deadline <= DateTime.UtcNow )
                return new BadRequestObjectResult(new ApiResponse(400, "Deadline must be in the future."));

            // Get from token
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // Validate Supervisor
            
            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));


            // Validate Task
            var task = await _unitOfWork.GetRepository<Task>()
                .GetAllAsync()
                .Include(t => t.Team)
                    .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.SupervisorId == supervisor.Id);
            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found or does not belong to the current supervisor."));


            // Validate assigned student is in team
            var isStudentInTeam = task.Team.TeamMembers.Any(m => m.Id == dto.AssignedToId);
            if ( !isStudentInTeam )
                return new BadRequestObjectResult(new ApiResponse(400, "Assigned student not found or is not in the team."));


            // update
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Deadline = dto.Deadline;
            task.Status = dto.Status;
            task.AssignedStudentId = dto.AssignedToId;

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Task updated successfully."));

        }

        #endregion UpdateTask Service

        #region DeleteTask Service

        public async Task<ActionResult> DeleteTaskAsync( int taskId, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
               .FirstOrDefaultAsync(s => s.Email == email);
            if ( supervisor == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));


            var task = await _unitOfWork.GetRepository<Task>()
                     .GetAllAsync()
                     .Include(t => t.Team)
                         .ThenInclude(t => t.TeamMembers)
                     .FirstOrDefaultAsync(t => t.Id == taskId && t.SupervisorId == supervisor.Id);
            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found or does not belong to the current supervisor."));

            await _unitOfWork.GetRepository<Task>().DeleteAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Task deleted successfully."));
        }

        #endregion DeleteTask Service

        #region GetTaskByID Service

        public async Task<ActionResult> GetTaskByIdAsync( int taskId, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
              .FirstOrDefaultAsync(s => s.Email == email);

            var student = supervisor == null ? await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == email) : null;

            if ( supervisor == null && student == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User not found or this task not for him."));

            var task = await _unitOfWork.GetRepository<Task>()
                   .GetAllAsync()
                   .Include(t => t.Team)
                   .Include(t => t.Supervisor)
                   .Include(t => t.AssignedStudent)
                   .FirstOrDefaultAsync(t => t.Id == taskId &&
                       ( supervisor != null ? t.SupervisorId == supervisor.Id : t.TeamId == student.Team.Id ));

            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found."));


            // Get the project idea by team id
            var projectIdea = await _unitOfWork.GetRepository<ProjectIdea>()
                    .GetAllAsync()
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

        public async Task<ActionResult> GetTasksByTeamIdAsync( int teamId, ClaimsPrincipal user)
        {

            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // Get the supervisor or student from token
            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
               .FirstOrDefaultAsync(s => s.Email == email);
            var student = supervisor == null ? await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == email) : null;

            // if not found !
            if ( supervisor == null && student == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User not found."));

            // validate they have access on this task
            if ( supervisor != null && !await _unitOfWork.GetRepository<Team>()
               .GetAllAsync()
               .AnyAsync(t => t.Id == teamId && t.SupervisorId == supervisor.Id) )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not the supervisor of this team."));
            if ( student != null && student.Team?.Id != teamId )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not part of this team."));

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

        public async Task<ActionResult> GetTasksBySupervisorIdAsync( int supervisorId, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( supervisor == null || supervisor.Id != supervisorId )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not authorized to view these tasks."));




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

        public async Task<ActionResult> GetTasksByStudentIdAsync( int assignedToId, ClaimsPrincipal user )
        {

            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( student == null || student.Id != assignedToId )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not authorized to view these tasks."));


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

        // date only not accurate

        #region FilterTasks Service

        public async Task<ActionResult> FilterTasksAsync( TaskFilterDto filter, ClaimsPrincipal user )
        {

            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )           
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));
           
            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
               .FirstOrDefaultAsync(s => s.Email == email);
            var student = supervisor == null ? await _unitOfWork.GetRepository<Student>()
                .GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == email) : null;
            if ( supervisor == null && student == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User not found."));



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
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var student = await _unitOfWork.GetRepository<Student>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(s => s.Email == email);
            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student not found from token."));


            // get task and verify it belongs to the student
            var task = await _unitOfWork.GetRepository<Task>()
                     .GetAllAsync()
                     .Include(t => t.Team)
                         .ThenInclude(t => t.TeamMembers)
                     .Include(t => t.Supervisor)
                     .Include(t => t.AssignedStudent)
                     .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedStudentId == student.Id);
            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found or not assigned to this student."));


            // cannot move to completed unless InProgress
            if ( newStatus == TaskStatusEnum.Completed && task.Status != TaskStatusEnum.InProgress )
                return new BadRequestObjectResult(new ApiResponse(400, "Task must be InProgress before marking as Done."));
            if ( newStatus == TaskStatusEnum.Approved || newStatus == TaskStatusEnum.Rejected )
                return new BadRequestObjectResult(new ApiResponse(400, "Students cannot set Approved or NeedToRevise status."));
            if ( task.Status == TaskStatusEnum.Approved || task.Status == TaskStatusEnum.Rejected )
                return new BadRequestObjectResult(new ApiResponse(400, "Task status cannot be changed after being Approved or NeedToRevise."));


            // Update task status
            task.Status = newStatus;
            await _unitOfWork.SaveChangesAsync();

            var projectIdea = await _unitOfWork.GetRepository<ProjectIdea>()
                    .GetAllAsync()
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

        #region ReviewTask Service
        public async Task<ActionResult> ReviewTaskAsync( int taskId, ReviewTaskDto dto, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( supervisor == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "Supervisor not found."));

            try
            {
                var task = await _unitOfWork.GetRepository<Task>()
                    .GetAllAsync()
                    .Include(t => t.AssignedStudent)
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.SupervisorId == supervisor.Id);
                if ( task == null )
                    return new NotFoundObjectResult(new ApiResponse(404, "Task not found or not assigned to this supervisor."));
                if ( task.Status != TaskStatusEnum.Completed )
                    return new BadRequestObjectResult(new ApiResponse(400, "Task must be Completed to be reviewed."));

                task.Status = dto.IsApproved ? TaskStatusEnum.Approved : TaskStatusEnum.Rejected;
                await _unitOfWork.SaveChangesAsync();

                var projectIdea = await _unitOfWork.GetRepository<ProjectIdea>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(p => p.TeamId == task.TeamId);

                var response = new TaskResponseDto
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Deadline = task.Deadline,
                    Status = task.Status.ToString(),
                    TeamName = task.Team?.Name,
                    SupervisorName = supervisor != null ? $"{supervisor.FirstName} {supervisor.LastName}" : null,
                    AssignedStudentName = task.AssignedStudent != null ? $"{task.AssignedStudent.FirstName} {task.AssignedStudent.LastName}" : null,
                    ProjectIdeaTitle = projectIdea?.Title,
                    Message = $"Task {( dto.IsApproved ? "approved" : "rejected" )} successfully"
                };

                return new OkObjectResult(response);
            }
            catch ( Exception )
            {
                return new ObjectResult(new ApiResponse(500, "An error occurred while reviewing the task."))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        #endregion ReviewTask Service

        #region SubmitTask Service
        public async Task<ActionResult> SubmitTaskAsync( int taskId, SubmitTaskServiceDto dto, ClaimsPrincipal user )
        {
            // Validate: At least one of FilePath or RepoLink must be provided
            if ( string.IsNullOrWhiteSpace(dto.FilePath) && string.IsNullOrWhiteSpace(dto.RepoLink) )
                return new BadRequestObjectResult(new ApiResponse(400, "At least one of FilePath or RepoLink is required"));

            // Get email from token
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            // Find student by email
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student not found from token."));

            var studentId = student.Id; // int Id from Students table

            // Validate task
            var task = await _context.Tasks
                .Include(t => t.AssignedStudent)
                .Include(t => t.Team)
                    .ThenInclude(t => t.ProjectIdeas)
                .Include(t => t.Supervisor)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if ( task == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Task not found"));

            if ( task.Status != TaskStatusEnum.InProgress )
                return new BadRequestObjectResult(new ApiResponse(400, "Task must be InProgress to submit or maybe already submitted before"));

            if ( task.AssignedStudentId != studentId )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not authorized to submit this task"));

            // Create task submission
            var submission = new TaskSubmission
            {
                TaskId = taskId,
                FilePath = dto.FilePath,
                Comments = dto.Comments,
                RepoLink = dto.RepoLink,
                SubmittedAt = DateTime.UtcNow
            };

            // Update task status to Completed
            task.Status = TaskStatusEnum.Completed;

            // Save submission and task
            try
            {
                _context.TaskSubmissions.Add(submission);
                await _context.SaveChangesAsync();
            }
            catch ( DbUpdateException ex )
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return new ObjectResult(new ApiResponse(500, $"Error saving task submission: {innerExceptionMessage}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch ( Exception ex )
            {
                return new ObjectResult(new ApiResponse(500, $"Unexpected error: {ex.Message}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            // Prepare response
            var projectIdea = task?.Team?.ProjectIdeas?.FirstOrDefault(pi => pi.Status == ProjectIdeaStatus.Accepted);
            var response = new
            {
                SubmissionId = submission.Id,
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Status = task.Status.ToString(),
                Team = new { task?.Team?.Id, task?.Team?.Name },
                Supervisor = new { task?.Supervisor?.Id, Name = $"{task?.Supervisor?.FirstName} {task?.Supervisor?.LastName}" },
                AssignedStudent = new { task?.AssignedStudent?.Id, Name = $"{task?.AssignedStudent?.FirstName} {task?.AssignedStudent?.LastName}" },
                ProjectIdea = projectIdea != null ? new { projectIdea.Id, projectIdea.Title } : null,
                Submission = new
                {
                    submission.FilePath,
                    submission.Comments,
                    submission.RepoLink,
                    submission.SubmittedAt
                },
                Message = "Task submitted successfully"
            };

            return new OkObjectResult(response);
        }

        #endregion SubmitTask Service

        #region ReassignTask Service
        public async Task<ActionResult> ReassignTaskAsync( int taskId, ReassignTaskDto dto, ClaimsPrincipal user )
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if ( string.IsNullOrEmpty(email) )
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if ( supervisor == null )
                return new UnauthorizedObjectResult(new ApiResponse(401, "Supervisor not found."));

            try
            {
                var task = await _unitOfWork.GetRepository<Task>()
                    .GetAllAsync()
                    .Include(t => t.Team)
                        .ThenInclude(t => t.TeamMembers)
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.SupervisorId == supervisor.Id);
                if ( task == null )
                    return new NotFoundObjectResult(new ApiResponse(404, "Task not found or not assigned to this supervisor."));

                if ( !task.Team.TeamMembers.Any(m => m.Id == dto.NewAssignedToId) )
                    return new BadRequestObjectResult(new ApiResponse(400, "New assigned student is not a member of the team."));

                task.AssignedStudentId = dto.NewAssignedToId;
                await _unitOfWork.SaveChangesAsync();

                var newStudent = await _unitOfWork.GetRepository<Student>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(s => s.Id == dto.NewAssignedToId);
                var projectIdea = await _unitOfWork.GetRepository<ProjectIdea>()
                    .GetAllAsync()
                    .FirstOrDefaultAsync(p => p.TeamId == task.TeamId);

                var response = new TaskResponseDto
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Deadline = task.Deadline,
                    Status = task.Status.ToString(),
                    TeamName = task.Team?.Name,
                    SupervisorName = supervisor != null ? $"{supervisor.FirstName} {supervisor.LastName}" : null,
                    AssignedStudentName = newStudent != null ? $"{newStudent.FirstName} {newStudent.LastName}" : null,
                    ProjectIdeaTitle = projectIdea?.Title,
                    Message = "Task reassigned successfully"
                };

                return new OkObjectResult(response);
            }
            catch ( Exception )
            {
                return new ObjectResult(new ApiResponse(500, "An error occurred while reassigning the task."))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        #endregion ReassignTask Service






    }
}