using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;
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

        #region RequestToJoinTeam 

        [HttpPost("JoinTeam")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestToJoinTeam([FromBody] TeamJoinRequestDto model)
        {
            return await _requestService.RequestToJoinTeamAsync(User, model);
        }

        #endregion Join Team

        #region RespondToJoinRequest

        [HttpPost("RespondToJoinRequest/{requestId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RespondToJoinRequest(int requestId, string decision)
        {
            return await _requestService.RespondToJoinRequestAsync(User, requestId, decision);
        }

        #endregion Respond to Join Requests

        #region GetTeamJoinRequests

        [HttpGet("team/{teamId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTeamJoinRequests(int teamId)
            => await _requestService.GetTeamJoinRequestsAsync(User, teamId);


        #endregion Get Join Requests

        #region RequestToSupervisor

        [HttpPost("RequestSupervisor")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestSupervisor([FromBody] SendProjectIdeaRequestDto dto)
        => await _requestService.RequestSupervisorAsync(User, dto);

        #endregion Send Idea Request To Supervisor


    }
}
