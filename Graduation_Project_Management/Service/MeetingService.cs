using Domain.Entities;
using Domain.Repository;
using Graduation_Project_Management.DTOs.MeetingDto;
using Graduation_Project_Management.Errors;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_Project_Management.Service
{

    public class MeetingsServices : IMeetingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MeetingsServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region GetAllMeetings Service
        public async Task<ActionResult> GetAllMeetingsAsync(ClaimsPrincipal user)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return new BadRequestObjectResult(new ApiResponse(400, "User email not found in token."));

            var meetings = await _unitOfWork.GetRepository<Meeting>()
                .GetAllAsync()
                .Include(m => m.Team)
                .Include(m => m.Supervisor)
                .ToListAsync();

            if (meetings == null || !meetings.Any())
                return new NotFoundObjectResult(new ApiResponse(404, "No meetings found."));

            var response = meetings.Select(async m =>
            {
                return new MeetingResponseDto
                {
                    MeetingId = m.Id,
                    Title = m.Title,
                    ScheduledAt = m.ScheduledAt,
                    MeetingLink = m.MeetingLink,
                    Objectives = m.Objectives,
                    Comment = m.Comment,
                    TeamName = m.Team?.Name,
                    SupervisorName = m.Supervisor != null ? $"{m.Supervisor.FirstName} {m.Supervisor.LastName}" : null,
                    Message = "Meeting Retrieved Successfully"
                };
            }).Select(m => m.Result).ToList();

            return new OkObjectResult(response);
        }
        #endregion GetAllMeetings Service

        #region CreateMeeting Service
        public async Task<ActionResult> CreateMeetingAsync(CreateMeetingDto dto, ClaimsPrincipal user)
        {
            // Validate DTO

            if (dto.ScheduledAt <= DateTime.UtcNow)
                return new BadRequestObjectResult(new ApiResponse(400, "Scheduled time must be in the future."));


            // Validate User
            var email = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if (supervisor == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

            // Validate Team
            var team = await _unitOfWork.GetRepository<Team>()
                .GetAllAsync()
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId && t.SupervisorId == supervisor.Id);
            if (team == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Invalid team ID or team does not belong to the current supervisor."));

            // Create Meeting
            var meeting = new Meeting
            {
                Title = dto.Title,
                ScheduledAt = dto.ScheduledAt,
                MeetingLink = dto.MeetingLink,
                Objectives = dto.Objectives,
                Comment = dto.Comment,
                TeamId = dto.TeamId,
                SupervisorId = supervisor.Id
            };

            await _unitOfWork.GetRepository<Meeting>().AddAsync(meeting);
            await _unitOfWork.SaveChangesAsync();

            // Prepare Response
            var response = new MeetingResponseDto
            {
                MeetingId = meeting.Id,
                Title = meeting.Title,
                ScheduledAt = meeting.ScheduledAt,
                MeetingLink = meeting.MeetingLink,
                Objectives = meeting.Objectives,
                Comment = meeting.Comment,
                TeamName = team.Name,
                SupervisorName = $"{supervisor.FirstName} {supervisor.LastName}",
                Message = "Meeting Created Successfully"
            };

            return new OkObjectResult(response);
        }
        #endregion CreateMeeting Service

        #region UpdateMeeting Service
        public async Task<ActionResult> UpdateMeetingAsync(int meetingId, UpdateMeetingDto dto, ClaimsPrincipal user)
        {
            // Validate DTO

            if (dto.ScheduledAt <= DateTime.UtcNow)
                return new BadRequestObjectResult(new ApiResponse(400, "Scheduled time must be in the future."));


            // Validate User
            var email = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if (supervisor == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

            // Validate Meeting
            var meeting = await _unitOfWork.GetRepository<Meeting>()
                .GetAllAsync()
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.SupervisorId == supervisor.Id);
            if (meeting == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Meeting not found or does not belong to the current supervisor."));



            // Update Meeting
            meeting.Title = dto.Title ?? meeting.Title;
            meeting.ScheduledAt = dto.ScheduledAt ?? meeting.ScheduledAt;
            meeting.MeetingLink = dto.MeetingLink ?? meeting.MeetingLink;
            meeting.Objectives = dto.Objectives ?? meeting.Objectives;
            meeting.Comment = dto.Comment ?? meeting.Comment;

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Meeting updated successfully."));
        }
        #endregion UpdateMeeting Service

        #region DeleteMeeting Service
        public async Task<ActionResult> DeleteMeetingAsync(int meetingId, ClaimsPrincipal user)
        {
            // Validate User
            var email = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return new UnauthorizedObjectResult(new ApiResponse(401, "User email not found in claims."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == email);
            if (supervisor == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Supervisor not found from token."));

            // Validate Meeting
            var meeting = await _unitOfWork.GetRepository<Meeting>()
                .GetAllAsync()
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.SupervisorId == supervisor.Id);
            if (meeting == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Meeting not found or does not belong to the current supervisor."));

            // Delete Meeting
            await _unitOfWork.GetRepository<Meeting>().DeleteAsync(meeting);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Meeting deleted successfully."));
        }
        #endregion DeleteMeeting Service

        #region GetMeetingById Service
        public async Task<ActionResult> GetMeetingByIdAsync(int meetingId, ClaimsPrincipal user)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return new BadRequestObjectResult(new ApiResponse(400, "User email not found in token."));

            var meeting = await _unitOfWork.GetRepository<Meeting>()
                .GetAllAsync()
                .Include(m => m.Team)
                .Include(m => m.Team.TeamMembers)
                .Include(m => m.Supervisor)
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Meeting not found."));
            var isSupervisor = meeting.Supervisor?.Email == userEmail;
            var isTeamMember = meeting.Team?.TeamMembers?.Any(m => m.Email == userEmail) == true;

            if (!isSupervisor && !isTeamMember)
                return new UnauthorizedObjectResult(new ApiResponse(401, "You are not authorized to view this meeting."));
            var response = new MeetingResponseDto
            {
                MeetingId = meeting.Id,
                Title = meeting.Title,
                ScheduledAt = meeting.ScheduledAt,
                MeetingLink = meeting.MeetingLink,
                Objectives = meeting.Objectives,
                Comment = meeting.Comment,
                TeamName = meeting.Team?.Name,
                SupervisorName = meeting.Supervisor != null ? $"{meeting.Supervisor.FirstName} {meeting.Supervisor.LastName}" : null,
                Message = "Meeting Retrieved Successfully"
            };

            return new OkObjectResult(response);
        }
        #endregion GetMeetingById Service

        #region GetMeetingsByTeamId Service
        public async Task<ActionResult> GetMeetingsByTeamIdAsync(int teamId, ClaimsPrincipal user)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return new BadRequestObjectResult(new ApiResponse(400, "User email not found in token."));

            var team = await _unitOfWork.GetRepository<Team>()
        .GetAllAsync()
        .Include(t => t.TeamMembers)
        .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Team not found."));

            var isMember = team.TeamMembers?.Any(m => m.Email == userEmail) == true;
            if (!isMember)
                return new UnauthorizedObjectResult(new ApiResponse(401, "You are not authorized to view this team's meetings."));


            var meetings = await _unitOfWork.GetRepository<Meeting>()
                .GetAllAsync()
                .Include(m => m.Team)
                .Include(m => m.Supervisor)
                .Where(m => m.TeamId == teamId)
                .ToListAsync();

            if (meetings == null || !meetings.Any())
                return new NotFoundObjectResult(new ApiResponse(200, "No meetings found for the specified team."));

            var response = meetings.Select(m => new MeetingResponseDto
            {
                MeetingId = m.Id,
                Title = m.Title,
                ScheduledAt = m.ScheduledAt,
                MeetingLink = m.MeetingLink,
                Objectives = m.Objectives,
                Comment = m.Comment,
                TeamName = m.Team?.Name,
                SupervisorName = m.Supervisor != null ? $"{m.Supervisor.FirstName} {m.Supervisor.LastName}" : null,
                Message = "Meeting Retrieved Successfully"
            }).ToList();

            return new OkObjectResult(response);
        }
        #endregion GetMeetingsByTeamId Service

        #region GetMeetingsBySupervisorId Service
        public async Task<ActionResult> GetMeetingsBySupervisorIdAsync(int supervisorId, ClaimsPrincipal user)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return new BadRequestObjectResult(new ApiResponse(400, "User email not found in token."));

            var supervisor = await _unitOfWork.GetRepository<Supervisor>()
                .GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (supervisor == null || supervisor.Id != supervisorId)
                return new UnauthorizedObjectResult(new ApiResponse(401, "Unauthorized access to supervisor's meetings."));

            var meetings = await _unitOfWork.GetRepository<Meeting>()
                .GetAllAsync()
                .Include(m => m.Team)
                .Include(m => m.Supervisor)
                .Where(m => m.SupervisorId == supervisorId)
                .ToListAsync();

            if (meetings == null || !meetings.Any())
                return new NotFoundObjectResult(new ApiResponse(404, "No meetings found for the specified supervisor."));

            var response = meetings.Select(m => new MeetingResponseDto
            {
                MeetingId = m.Id,
                Title = m.Title,
                ScheduledAt = m.ScheduledAt,
                MeetingLink = m.MeetingLink,
                Objectives = m.Objectives,
                Comment = m.Comment,
                TeamName = m.Team?.Name,
                SupervisorName = m.Supervisor != null ? $"{m.Supervisor.FirstName} {m.Supervisor.LastName}" : null,
                Message = "Meeting Retrieved Successfully"
            }).ToList();

            return new OkObjectResult(response);
        }
        #endregion GetMeetingsBySupervisorId Service

    }
}


