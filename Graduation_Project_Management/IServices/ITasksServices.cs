using Domain.Entities;
using Domain.Enums;
using Graduation_Project_Management.DTOs.TasksDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface ITasksServices
    {
        Task<ActionResult> CreateTaskAsync( CreateTaskDto taskDto, ClaimsPrincipal user );

        Task<ActionResult> UpdateTaskAsync( int taskId, UpdateTaskDto dto, ClaimsPrincipal user );

        Task<ActionResult> DeleteTaskAsync( int taskId, ClaimsPrincipal user );

        Task<ActionResult> GetTaskByIdAsync( int taskId );

        Task<ActionResult> GetAllTasksAsync();

        Task<ActionResult> GetTasksByTeamIdAsync( int teamId );

        Task<ActionResult> GetTasksBySupervisorIdAsync( int supervisorId );

        Task<ActionResult> GetTasksByStudentIdAsync( int assignedToId );

        Task<ActionResult> FilterTasksAsync( TaskFilterDto filter );

        Task<ActionResult> ChangeTaskStatusAsync( int taskId, TaskStatusEnum newStatus, ClaimsPrincipal user );
    }
}