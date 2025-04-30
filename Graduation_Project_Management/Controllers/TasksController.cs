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

        public TasksController( ITasksServices tasksServices )
        {
            _tasksServices = tasksServices;
        }

        // maybe for admin

        #region GetAllTasks

        [HttpGet]
        public async Task<ActionResult> GetAllTasks()
        => await _tasksServices.GetAllTasksAsync();

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
            => await _tasksServices.GetTaskByIdAsync(taskId);

        #endregion GetTaskById

        #region GetTasksByTeamId

        [HttpGet("team/{teamId}")]
        public async Task<ActionResult> GetTasksByTeamId( int teamId )
        => await _tasksServices.GetTasksByTeamIdAsync(teamId);

        #endregion GetTasksByTeamId

        #region GetTasksBySupervisorId

        [HttpGet("supervisor/{supervisorId}")]
        public async Task<ActionResult> GetTasksBySupervisorId( int supervisorId )
        => await _tasksServices.GetTasksBySupervisorIdAsync(supervisorId);

        #endregion GetTasksBySupervisorId

        #region GetTasksByStudentId

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetTasksByStudentId( int studentId )
            => await _tasksServices.GetTasksByStudentIdAsync(studentId);

        #endregion GetTasksByStudentId

        #region FilterTasks

        [HttpGet("Filter")]
        public async Task<ActionResult> FilterTasks( [FromQuery] TaskFilterDto filter )
            => await _tasksServices.FilterTasksAsync(filter);

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
    }
}