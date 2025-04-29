using Graduation_Project_Management.DTOs.TasksDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface ITasksServices
    {
        Task<ActionResult> CreateTaskAsync( CreateTaskDto taskDto, ClaimsPrincipal user );

        //Task<bool> UpdateTaskAsync( int taskId, UpdateTaskDto taskDto );
        //Task<bool> DeleteTaskAsync( int taskId );
        //Task<TaskDto> GetTaskByIdAsync( int taskId );
        //Task<List<TaskDto>> GetAllTasksAsync();
        //Task<List<TaskDto>> GetTasksByTeamIdAsync( int teamId );
        //Task<List<TaskDto>> GetTasksBySupervisorIdAsync( int supervisorId );
        //Task<List<TaskDto>> GetTasksByAssignedToIdAsync( int assignedToId );
        //Task<List<TaskDto>> FilterTasksAsync( TaskFilterDto filter );
    }
}