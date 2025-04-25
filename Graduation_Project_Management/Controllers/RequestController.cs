using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        #region  Dependencies

        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        #endregion  Dependencies

        #region Get Join Requests

        [HttpGet("team/{teamId}/join-requests")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTeamJoinRequests(int teamId)
        {
            return await _requestService.GetTeamJoinRequestsAsync(User, teamId);
        }

        #endregion Get Join Requests

        #region Respond to Join Requests

        [HttpPost("join-requests/{requestId}/respond")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RespondToJoinRequest(int requestId, string decision)
        {
            return await _requestService.RespondToJoinRequestAsync(User, requestId, decision);
        }

        #endregion Respond to Join Requests

        #region Join 

        [HttpPost("JoinTeam")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestToJoinTeam([FromBody] TeamJoinRequestDto model)
        {
            return await _requestService.RequestToJoinTeamAsync(User, model);
        }

        #endregion Join Team

        #region Send Idea Request To Supervisor

        [HttpPost("RequestSupervisor")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestSupervisor([FromBody] SendProjectIdeaRequestDto dto)
        => await _requestService.RequestSupervisorAsync(User, dto);

        #endregion Send Idea Request To Supervisor
    }
}
