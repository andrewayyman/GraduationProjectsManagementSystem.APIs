using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IRequestService
    {
        Task<ActionResult> GetTeamJoinRequestsAsync(ClaimsPrincipal user, int teamId);
        Task<ActionResult> RespondToJoinRequestAsync(ClaimsPrincipal user, int requestId, string decision);

        Task<ActionResult> RequestToJoinTeamAsync(ClaimsPrincipal user, TeamJoinRequestDto model);
        Task<IActionResult> RequestSupervisorAsync(ClaimsPrincipal user, SendProjectIdeaRequestDto dto);
    }
}
