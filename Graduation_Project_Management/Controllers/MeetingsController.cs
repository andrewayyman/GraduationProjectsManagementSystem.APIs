using Graduation_Project_Management.DTOs.MeetingDto;
using Graduation_Project_Management.Errors;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingsServices;

        public MeetingsController(IMeetingService meetingsServices)
        {
            _meetingsServices = meetingsServices;
        }

        #region Get All Meetings
        [HttpGet]
        public async Task<ActionResult> GetAllMeetings()
        {
            var result = await _meetingsServices.GetAllMeetingsAsync(User);
            return result;
        }
        #endregion

        #region Create Meeting
        [HttpPost]
        public async Task<ActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, "Invalid meeting data."));

            var result = await _meetingsServices.CreateMeetingAsync(dto, User);
            return result;
        }
        #endregion

        #region Update Meeting
        [HttpPut("{meetingId}")]
        public async Task<ActionResult> UpdateMeeting(int meetingId, [FromBody] UpdateMeetingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, "Invalid meeting data."));

            var result = await _meetingsServices.UpdateMeetingAsync(meetingId, dto, User);
            return result;
        }
        #endregion

        #region Delete Meeting
        [HttpDelete("{meetingId}")]
        public async Task<ActionResult> DeleteMeeting(int meetingId)
        {
            var result = await _meetingsServices.DeleteMeetingAsync(meetingId, User);
            return result;
        }
        #endregion

        #region Get Meeting By Id
        [HttpGet("{meetingId}")]
        public async Task<ActionResult> GetMeetingById(int meetingId)
        {
            var result = await _meetingsServices.GetMeetingByIdAsync(meetingId, User);
            return result;
        }
        #endregion

        #region Get Meetings By Team Id
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult> GetMeetingsByTeamId(int teamId)
        {
            var result = await _meetingsServices.GetMeetingsByTeamIdAsync(teamId, User);
            return result;
        }
        #endregion

        #region Get Meetings By Supervisor Id
        [HttpGet("supervisor/{supervisorId}")]
        public async Task<ActionResult> GetMeetingsBySupervisorId(int supervisorId)
        {
            var result = await _meetingsServices.GetMeetingsBySupervisorIdAsync(supervisorId, User);
            return result;
        }
        #endregion
    }
}

