using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.DTOs.TeamsDtos;
using Graduation_Project_Management.DTOs.TeamsDTOs;
using Graduation_Project_Management.Errors;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Service
{
    public class TeamService : ITeamService
    {
        #region Dependencies
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Get All
        public async Task<IActionResult> GetAvailableTeamsAsync()
        {
            var teams = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Include(t => t.TeamMembers)
                .Include(t => t.ProjectIdeas)
                    .ThenInclude(p => p.Supervisor)
                .Where(t => t.TeamMembers.Count < 6)
                .ToListAsync();

            var result = teams.Select(t => new GetTeamsDto
            {
                TeamId = t.Id,
                Name = t.Name,
                Description = t.Description,
                TeamDepartment = t.TeamDepartment,
                TechStack = t.TechStack,
                MembersCount = t.TeamMembers.Count,

                TeamMembers = t.TeamMembers
                    .Select(m => new TeamMemberDto
                    {
                        Id = m.Id,
                        FullName = m.FirstName + " " + m.LastName,
                    })
                    .ToList(),

                ProjectIdeas = t.ProjectIdeas.Select(p => new ProjectIdeaDto
                {
                    ProjectIdeaId = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    TechStack = p.TechStack,
                    CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd"),
                    Status = p.Status.ToString(),
                    Supervisor = p.Supervisor != null ? new ShortSupervisorDto
                    {
                        SupervisorId = p.Supervisor.Id,
                        Name = p.Supervisor.FirstName + " " + p.Supervisor.LastName
                    } : null
                }).ToList()
            }).ToList();

            return new OkObjectResult(result);
        }


        #endregion

        #region Create
        public async Task<IActionResult> CreateTeamAsync(ClaimsPrincipal user, TeamDto dto)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var CreatorStudent = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Where(s => s.Email == userEmail)
                .Include(S => S.Team)
                .FirstOrDefaultAsync();

            // Validate TeamCreator
            if (CreatorStudent == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Student profile not found."));

            if (CreatorStudent.Team != null)
                return new BadRequestObjectResult(new ApiResponse(400, "You are already part of another team."));

            // Validate Added TeamMembers
            var TeamMembers = new List<Student>() { CreatorStudent };
            if (dto.MembersEmails != null && dto.MembersEmails.Any())
            {

                // to avoid duplicates and the creator's email
                var UniqueEmails = dto.MembersEmails
                    .Distinct()
                    .Where(email => email != userEmail)
                    .ToList();

                // check team max size
                if (UniqueEmails.Count + TeamMembers.Count > 6)
                    return new BadRequestObjectResult(new ApiResponse(400, "Team members exceed the maximum limit of 6."));

                // Fetch all students with the provided emails
                var students = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                    .Where(s => UniqueEmails.Contains(s.Email))
                    .Include(s => s.Team)
                    .ToListAsync();

                // Check if all requested students exist
                if (students.Count != UniqueEmails.Count)
                    return new BadRequestObjectResult(new ApiResponse(400, "One or more students not found."));

                // Check if any student is already in a team
                foreach (var student in students)
                {
                    if (student.Team != null)
                        return new BadRequestObjectResult(new ApiResponse(400, $"Student {student.FirstName} {student.LastName} is already in a team."));
                    TeamMembers.Add(student);
                }


            }


            var team = new Team
            {
                Name = dto.Name,
                Description = dto.Description,
                TeamDepartment = dto.TeamDepartment,
                TechStack = dto.TechStack ?? new List<string>(),
                TeamMembers = TeamMembers
            };

            await _unitOfWork.GetRepository<Team>().AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            var response = new
            {
                teamId = team.Id,
                apiResponse = new ApiResponse(200, "Team created successfully.")
            };
            return new OkObjectResult(response);
        }
        #endregion

        #region Get Team By Id
        public async Task<IActionResult> GetTeamByIdAsync(int id, ClaimsPrincipal user)
        {
            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                      .Include(t => t.TeamMembers)
                      .Include(t => t.ProjectIdeas)
                            .ThenInclude(p => p.Supervisor)
                      .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Team not found"));

            var userEmail = user.FindFirstValue(ClaimTypes.Email);


            var isTeamMember = team.TeamMembers.Any(m => m.Email == userEmail);
            var isSupervisorOfTeam = team.ProjectIdeas.Any(p => p.Supervisor != null && p.Supervisor.Email == userEmail);

            if (!isTeamMember && !isSupervisorOfTeam)
                return new ObjectResult(new ApiResponse(403, "You are not authorized to view this team."))
                { StatusCode = StatusCodes.Status403Forbidden };

            var result = new GetTeamWithRoleDto
            {
                TeamId = team.Id,
                Name = team.Name,
                Description = team.Description,
                TeamDepartment = team.TeamDepartment,
                TechStack = team.TechStack,
                MembersCount = team.TeamMembers.Count,
                TeamMembers = team.TeamMembers
                    .Select(m => new TeamDtoWithRole
                    {
                        Id = m.Id,
                        FullName = $"{m.FirstName} {m.LastName}",
                        MainRole = m.MainRole
                    })
                    .ToList(),
                ProjectIdeas = team.ProjectIdeas.Select(p => new ProjectIdeaDto
                {
                    ProjectIdeaId = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    TechStack = p.TechStack,
                    CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd"),
                    Status = p.Status.ToString(),
                    Supervisor = p.Supervisor != null ? new ShortSupervisorDto
                    {
                        SupervisorId = p.Supervisor.Id,
                        Name = p.Supervisor.FirstName + " " + p.Supervisor.LastName,
                    } : null
                }).ToList()
            };

            return new OkObjectResult(result);
        }
        #endregion


        #region Get Team By StudentId
        public async Task<IActionResult> GetTeamByStudentIdAsync( int studentId, ClaimsPrincipal user )
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);

            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                 .Where(s => s.Id == studentId)
                 .Include(s => s.Team)
                     .ThenInclude(t => t.TeamMembers)
                 .Include(s => s.Team)
                     .ThenInclude(t => t.ProjectIdeas)
                         .ThenInclude(p => p.Supervisor)
                 .FirstOrDefaultAsync();

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404, "Student not found"));

            var team = student.Team;

            if ( team == null )
                return new ObjectResult(new ApiResponse(200, "This student does not belong to any team yet"));

            var isStudentSelf = student.Email == userEmail;
            var isSupervisor = team.ProjectIdeas.Any(p => p.Supervisor != null && p.Supervisor.Email == userEmail);

            if ( !isStudentSelf && !isSupervisor )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not authorized to view this team."));

            var isCompleted = team.ProjectIdeas?.Any(pi => pi.IsCompleted) ?? false; // Check if any project idea is completed

            var result = new GetTeamByStudentIdWithCompletionDto
            {
                StudentName = student.FirstName + " " + student.LastName,
                TeamId = team.Id,
                Name = team.Name,
                Description = team.Description,
                TeamDepartment = team.TeamDepartment,
                TechStack = team.TechStack,
                MembersCount = team.TeamMembers.Count,
                TeamMembers = team.TeamMembers
                    .Select(m => new TeamDtoWithRole
                    {
                        Id = m.Id,
                        FullName = $"{m.FirstName} {m.LastName}",
                        MainRole = m.MainRole
                    })
                    .ToList(),
                ProjectIdeas = team.ProjectIdeas.Select(p => new ProjectIdeaDto
                {
                    ProjectIdeaId = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    TechStack = p.TechStack,
                    CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd"),
                    Status = p.Status.ToString(),
                    Supervisor = p.Supervisor != null ? new ShortSupervisorDto
                    {
                        SupervisorId = p.Supervisor.Id,
                        Name = p.Supervisor.FirstName + " " + p.Supervisor.LastName,
                    } : null,
                }).ToList(),
                IsCompleted = isCompleted 
            };

            return new OkObjectResult(result);
        }
        #endregion


        #region Update Team Profile
        public async Task<IActionResult> UpdateTeamProfileAsync(ClaimsPrincipal user, UpdateTeamDto dto)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);

            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                    .Where(s => s.Email == userEmail)
                    .Include(s => s.Team)
                    .FirstOrDefaultAsync();

            if (student == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Student not found"));

            if (student.Team == null)
                return new BadRequestObjectResult(new ApiResponse(400, "You are not a member of any team"));

            var team = student.Team;

            team.Name = dto.Name ?? team.Name;
            team.Description = dto.Description ?? team.Description;
            team.TeamDepartment = dto.TeamDepartment ?? team.TeamDepartment;
            team.TechStack = dto.TechStack ?? team.TechStack;

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Team profile updated successfully"));
        }
        #endregion

        #region Delete 
        public async Task<IActionResult> DeleteTeamAsync(ClaimsPrincipal user, int teamId)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var team = await _unitOfWork.GetRepository<Team>().GetAllAsync()
                .Where(t => t.Id == teamId)
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync();

            if (team == null)
                return new NotFoundObjectResult(new ApiResponse(404, "Team not found"));

            if (!team.TeamMembers.Any(m => m.Email == userEmail))
                return new ObjectResult(new ApiResponse(403, "You are not part of this team"));

            await _unitOfWork.GetRepository<Team>().DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200, "Team deleted successfully"));
        }
        #endregion







    }
}


