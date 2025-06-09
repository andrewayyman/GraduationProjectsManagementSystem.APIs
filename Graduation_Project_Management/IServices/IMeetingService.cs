using Graduation_Project_Management.DTOs.MeetingDto;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IMeetingService
    {
        Task<ActionResult> GetAllMeetingsAsync(ClaimsPrincipal user);
        Task<ActionResult> CreateMeetingAsync(CreateMeetingDto meetingDto, ClaimsPrincipal user);
        Task<ActionResult> UpdateMeetingAsync(int meetingId, UpdateMeetingDto dto, ClaimsPrincipal user);
        Task<ActionResult> DeleteMeetingAsync(int meetingId, ClaimsPrincipal user);
        Task<ActionResult> GetMeetingByIdAsync(int meetingId, ClaimsPrincipal user);
        Task<ActionResult> GetMeetingsByTeamIdAsync(int teamId, ClaimsPrincipal user);
        Task<ActionResult> GetMeetingsBySupervisorIdAsync(int supervisorId, ClaimsPrincipal user);
    }
}
