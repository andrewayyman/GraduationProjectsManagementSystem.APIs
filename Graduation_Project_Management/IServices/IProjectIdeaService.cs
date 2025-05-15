using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IProjectIdeaService
    {
        Task<IActionResult> PublishProjectIdeaAsync(ClaimsPrincipal user, PublishProjectIdeaDto dto);
        Task<IActionResult> DeleteIdeaAsync(ClaimsPrincipal user, int ideaId);
        Task<IActionResult> UpdateIdeaAsync(ClaimsPrincipal user, int ideaId, PublishProjectIdeaDto dto);

    }
}
