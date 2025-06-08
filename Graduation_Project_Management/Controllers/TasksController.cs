using Graduation_Project_Management.IServices;
using Graduation_Project_Management.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Graduation_Project_Management.IServices;

using Graduation_Project_Management.DTOs.TasksDTOs;
using AutoMapper;
using Graduation_Project_Management.Errors;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITasksServices _tasksServices;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        public TasksController( ITasksServices tasksServices )
        {
            _tasksServices = tasksServices;
            // Ensure upload directory exists
            if ( !Directory.Exists(_uploadPath) )
                Directory.CreateDirectory(_uploadPath);
        }

        // maybe for admin

        #region GetAllTasks

        [HttpGet]
        public async Task<ActionResult> GetAllTasks()
        => await _tasksServices.GetAllTasksAsync(User);

        #endregion GetAllTasks

        #region CreateTask

        [HttpPost]
        public async Task<ActionResult> CreateTask( [FromBody] CreateTaskDto dto )
            => await _tasksServices.CreateTaskAsync(dto, User);

        #endregion CreateTask

        // from supervisor

        #region UpdateTask

        [HttpPut("{taskId}")]
        public async Task<ActionResult> UpdateTask( int taskId, [FromBody] UpdateTaskDto dto )
        => await _tasksServices.UpdateTaskAsync(taskId, dto, User);

        #endregion UpdateTask

        #region DeleteTask

        [HttpDelete("{taskId}")]
        public async Task<ActionResult> DeleteTask( int taskId )
        => await _tasksServices.DeleteTaskAsync(taskId, User);

        #endregion DeleteTask

        #region GetTaskById

        [HttpGet("{taskId}")]
        public async Task<ActionResult> GetTaskById( int taskId )
            => await _tasksServices.GetTaskByIdAsync(taskId,User);

        #endregion GetTaskById

        #region GetTasksByTeamId

        [HttpGet("team/{teamId}")]
        public async Task<ActionResult> GetTasksByTeamId( int teamId )
        => await _tasksServices.GetTasksByTeamIdAsync(teamId, User);

        #endregion GetTasksByTeamId

        #region GetTasksBySupervisorId

        [HttpGet("supervisor/{supervisorId}")]
        public async Task<ActionResult> GetTasksBySupervisorId( int supervisorId )
        => await _tasksServices.GetTasksBySupervisorIdAsync(supervisorId, User);

        #endregion GetTasksBySupervisorId

        #region GetTasksByStudentId

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetTasksByStudentId( int studentId )
            => await _tasksServices.GetTasksByStudentIdAsync(studentId, User);

        #endregion GetTasksByStudentId

        #region FilterTasks

        [HttpGet("Filter")]
        public async Task<ActionResult> FilterTasks( [FromQuery] TaskFilterDto filter )
            => await _tasksServices.FilterTasksAsync(filter,User);

        #endregion FilterTasks

        // from student
        #region ChangeTaskStatus

        [HttpPut("{taskId}/status")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> ChangeTaskStatus( int taskId, [FromBody] ChangeTaskStatusDto dto )
        {
            if ( !ModelState.IsValid )
                return BadRequest(new ApiResponse(400, "Invalid input data"));

            if ( !Enum.TryParse<TaskStatusEnum>(dto.Status, true, out var newStatus) )
                return BadRequest(new ApiResponse(400, $"Invalid task status: {dto.Status}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(TaskStatusEnum)))}"));

            return await _tasksServices.ChangeTaskStatusAsync(taskId, newStatus, User);
        }

        #endregion ChangeTaskStatus

        // From supervisor
        #region ReviewTask
        [HttpPut("{taskId}/review")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult> ReviewTask( int taskId, [FromBody] ReviewTaskDto dto )
        {
            if ( !ModelState.IsValid )
                return BadRequest(new ApiResponse(400, "Invalid input data"));

            return await _tasksServices.ReviewTaskAsync(taskId, dto, User);
        }
        #endregion ReviewTask

        // From student
        #region SubmitTask
        [HttpPost("{taskId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> SubmitTask( int taskId, [FromForm] SubmitTaskDto dto )
        {
            if ( !ModelState.IsValid )
                return BadRequest(new ApiResponse(400, "Invalid input data"));

            // Validate: At least one of File or RepoLink must be provided
            if ( dto.File == null && string.IsNullOrWhiteSpace(dto.RepoLink) )
                return BadRequest(new ApiResponse(400, "At least one of File or RepoLink is required"));

            string? filePath = null;
            // Validate file if provided
            if ( dto.File != null )
            {
                if ( dto.File.Length == 0 )
                    return BadRequest(new ApiResponse(400, "File cannot be empty"));

                var allowedExtensions = new[] { ".pdf", ".docx", ".zip", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(dto.File.FileName).ToLower();
                if ( !allowedExtensions.Contains(fileExtension) )
                    return BadRequest(new ApiResponse(400, "Only .pdf, .docx, .zip, .jpg, .jpeg, or .png files are allowed"));

                if ( dto.File.Length > 5 * 1024 * 1024 ) // 5MB
                    return BadRequest(new ApiResponse(400, "File size must not exceed 5MB"));

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                filePath = Path.Combine(_uploadPath, fileName);

                // Save file to local storage
                try
                {
                    using ( var stream = new FileStream(filePath, FileMode.Create) )
                    {
                        await dto.File.CopyToAsync(stream);
                    }
                }
                catch ( Exception )
                {
                    return StatusCode(500, new ApiResponse(500, "Error saving file"));
                }

                filePath = $"/Uploads/{fileName}";
            }

            // Prepare DTO for service
            var submitTaskServiceDto = new SubmitTaskServiceDto
            {
                FilePath = filePath,
                Comments = dto.Comments,
                RepoLink = dto.RepoLink
            };

            return await _tasksServices.SubmitTaskAsync(taskId, submitTaskServiceDto, User);
        }
        #endregion SubmitTask

        #region GetTaskSubmission

        [HttpGet("{taskId}/submission")]
        [Authorize(Roles = "Supervisor,Student")]
        public async Task<ActionResult> GetTaskSubmission( int taskId )
            => await _tasksServices.GetTaskSubmissionAsync(taskId, User);

        #endregion GetTaskSubmission

        #region ReassignTask
        [HttpPut("{taskId}/reassign")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult> ReassignTask( int taskId, [FromBody] ReassignTaskDto dto )
        {
            if ( !ModelState.IsValid )
                return BadRequest(new ApiResponse(400, "Invalid input data"));

            return await _tasksServices.ReassignTaskAsync(taskId, dto, User);
        }
        #endregion ReassignTask



        #region MarkProjectAsCompleted
        
        [HttpPost("Project/{projectIdeaId}/Complete")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult> MarkProjectAsCompleted( int projectIdeaId )
            => await _tasksServices.MarkProjectAsCompletedAsync(projectIdeaId, User);


        #endregion MarkProjectAsCompleted

        
        #region GetCompletedProjectsBySupervisor
        
        [HttpGet("Supervisor/{supervisorId}/CompletedProjects")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult> GetCompletedProjectsBySupervisor( int supervisorId )
            => await _tasksServices.GetCompletedProjectsBySupervisorAsync(supervisorId, User);
        
        #endregion GetCompletedProjectsBySupervisor
    }
}