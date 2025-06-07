using Domain.Entities;
using Domain.Enums;
using Graduation_Project_Management.DTOs.TasksDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface ITasksServices
    {
        Task<ActionResult> GetAllTasksAsync( ClaimsPrincipal user );
        Task<ActionResult> CreateTaskAsync( CreateTaskDto taskDto, ClaimsPrincipal user );

        Task<ActionResult> UpdateTaskAsync( int taskId, UpdateTaskDto dto, ClaimsPrincipal user );

        Task<ActionResult> DeleteTaskAsync( int taskId, ClaimsPrincipal user );

        Task<ActionResult> GetTaskByIdAsync( int taskId, ClaimsPrincipal user );

        Task<ActionResult> GetTasksByTeamIdAsync( int teamId, ClaimsPrincipal user );

        Task<ActionResult> GetTasksBySupervisorIdAsync( int supervisorId, ClaimsPrincipal user );

        Task<ActionResult> GetTasksByStudentIdAsync( int assignedToId, ClaimsPrincipal user );

        Task<ActionResult> FilterTasksAsync( TaskFilterDto filter, ClaimsPrincipal user );

        Task<ActionResult> ChangeTaskStatusAsync( int taskId, TaskStatusEnum newStatus, ClaimsPrincipal user );



        //Task<ActionResult> SubmitTaskAsync( int taskId, SubmitTaskDto dto, ClaimsPrincipal user );
        //Task<ActionResult> ReviewTaskAsync( int taskId, ReviewTaskDto dto, ClaimsPrincipal user );
        //Task<ActionResult> GetTaskSubmissionsAsync( int taskId, ClaimsPrincipal user );
        //Task<ActionResult> ReassignTaskAsync( int taskId, ReassignTaskDto dto, ClaimsPrincipal user );

    }
}