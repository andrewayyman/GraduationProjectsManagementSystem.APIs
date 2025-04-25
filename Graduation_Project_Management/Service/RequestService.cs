using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Service
{
    public class RequestService : IRequestService
    {

        #region Dependencies
        private readonly IUnitOfWork _unitOfWork;

        public RequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Get 
        public async Task<ActionResult> GetTeamJoinRequestsAsync(ClaimsPrincipal user, int teamId)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student?.TeamId != teamId)
                return new ObjectResult("You are not part of this team") { StatusCode = StatusCodes.Status403Forbidden };

            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.JoinRequests)
                .ThenInclude(r => r.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return new NotFoundObjectResult("Team not Found");

            var requests = team.JoinRequests?.Select(r => new
            {
                r.Id,
                r.StudentId,
                StudentName = r.Student.User.UserName,
                r.Message,
                Status = Enum.GetName(typeof(JoinRequestStatus), r.Status),
                r.CreatedAt
            }).ToList();

            return new OkObjectResult(requests);
        }
        #endregion

        #region Respond
        public async Task<ActionResult> RespondToJoinRequestAsync(ClaimsPrincipal user, int requestId, string decision)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s => s.Team)
                .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student?.Team == null)
                return new ObjectResult("You are not part of this team") { StatusCode = StatusCodes.Status403Forbidden };

            var request = await _unitOfWork.GetRepository<TeamJoinRequest>().GetAllAsync()
                .Include(r => r.Team)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.TeamId != student.Team.Id)
                return new NotFoundObjectResult("Cannot Find The Request");

            if (request.Status != JoinRequestStatus.Pending)
                return new BadRequestObjectResult("Request has already been responded to.");

            if (decision.ToLower() == "accept")
            {
                if (request.Team.TeamMembers?.Count >= request.Team.MaxMembers)
                    return new BadRequestObjectResult("Team is Full");

                request.Status = JoinRequestStatus.Accepted;
                request.Team.TeamMembers.Add(request.Student);
            }
            else if (decision.ToLower() == "reject")
            {
                request.Status = JoinRequestStatus.Rejected;
            }
            else
            {
                return new BadRequestObjectResult("The Request Must be Rejected Or Accepted");
            }

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new { message = $"The request has been {request.Status}" });
        }
        #endregion

        #region Send Idea
        public async Task<IActionResult> RequestSupervisorAsync(ClaimsPrincipal user, SendProjectIdeaRequestDto dto)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync().Include(s => s.Team).FirstOrDefaultAsync(s => s.Email == email);

            if (student?.Team == null) return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var idea = await _unitOfWork.GetRepository<ProjectIdea>().GetAllAsync().FirstOrDefaultAsync(i => i.Id == dto.ProjectIdeaId && i.TeamId == student.Team.Id);
            if (idea == null) return new NotFoundObjectResult("Idea not found");

            var supervisor = await _unitOfWork.GetRepository<Supervisor>().GetByIdAsync(dto.SupervisorId);
            if (supervisor == null) return new NotFoundObjectResult("Supervisor not found");

            var request = new ProjectIdeaRequest
            {
                ProjectIdeaId = idea.Id,
                SupervisorId = dto.SupervisorId,
                Status = ProjectIdeaStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<ProjectIdeaRequest>().AddAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return new OkObjectResult(new { message = "Request sent to supervisor" });
        }
        #endregion

        #region Send Request
        public async Task<ActionResult> RequestToJoinTeamAsync(ClaimsPrincipal user, TeamJoinRequestDto model)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student == null)
                return new NotFoundObjectResult("Student profile not found.");

            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.JoinRequests)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId);

            if (team == null)
                return new NotFoundObjectResult("Team not found.");

            if (team.TeamMembers?.Any(m => m.Id == student.Id) == true)
                return new BadRequestObjectResult("You are already a member of this team.");

            if (team.JoinRequests?.Any(r => r.StudentId == student.Id) == true)
                return new BadRequestObjectResult("You already have a pending request to this team.");

            var joinRequest = new TeamJoinRequest
            {
                StudentId = student.Id,
                TeamId = team.Id,
                Message = model.Message,
                Status = JoinRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<TeamJoinRequest>().AddAsync(joinRequest);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new { message = "Join request sent successfully." });
        } 
        #endregion
    }
}
