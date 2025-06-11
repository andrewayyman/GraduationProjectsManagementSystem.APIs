using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface ISupervisorService
    {
        Task<ActionResult> GetAllSupervisorsAsync();

        Task<ActionResult> GetSupervisorByIdAsync( int id );
        Task<ActionResult> GetSupervisorByEmailAsync( string email );

        Task<ActionResult> UpdateSupervisorProfileAsync( int id, UpdateSupervisorDto supervisorDto );

        Task<ActionResult> DeleteSupervisorProfileAsync( int id, ClaimsPrincipal user );

        Task<ActionResult> GetPendingRequestsAsync( ClaimsPrincipal user );

        Task<ActionResult> GetMyTeamsAsync( ClaimsPrincipal user );

        Task<ActionResult> HandleIdeaRequestAsync( ClaimsPrincipal user, HandleIdeaRequestDto dto );
    }
}