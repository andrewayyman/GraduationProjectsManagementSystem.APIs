using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IProjectIdeaService
    {
        Task<IActionResult> PublishProjectIdeaAsync(ClaimsPrincipal user, PublishProjectIdeaDto dto);
        Task<IActionResult> GetAllTeamIdeasByStudentIdAsync( ClaimsPrincipal user, int studentId );
        Task<IActionResult> UpdateIdeaAsync(ClaimsPrincipal user, int ideaId, PublishProjectIdeaDto dto);
        Task<IActionResult> DeleteIdeaAsync(ClaimsPrincipal user, int ideaId);

    }
}
