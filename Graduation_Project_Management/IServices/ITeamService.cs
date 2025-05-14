using Graduation_Project_Management.DTOs.TeamsDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface ITeamService
    {
        Task<IActionResult> GetAvailableTeamsAsync();
        Task<IActionResult> CreateTeamAsync(ClaimsPrincipal user, TeamDto dto );
        Task<IActionResult> GetTeamByIdAsync(int id, ClaimsPrincipal user );
        Task<IActionResult> GetTeamByStudentIdAsync( int studentId, ClaimsPrincipal user );
        Task<IActionResult> UpdateTeamProfileAsync(ClaimsPrincipal user, UpdateTeamDto dto);
        Task<IActionResult> DeleteTeamAsync(ClaimsPrincipal user, int teamId);


    }
}
