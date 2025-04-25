using Graduation_Project_Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface ITeamService
    {
        Task<ActionResult> CreateTeamAsync(ClaimsPrincipal user, TeamDto model);
        Task<IActionResult> DeleteTeamAsync(ClaimsPrincipal user, int teamId);
        Task<IActionResult> GetAvailableTeamsAsync();
       
      
    }
}
