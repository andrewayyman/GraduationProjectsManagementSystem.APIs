using Domain.Entities.Identity;
using Domain.Entities;
using Graduation_Project_Management.DTOs.AuthDTOs;
using Graduation_Project_Management.DTOs.StudentDTOs;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.DTOs.TasksDTOs;
using Graduation_Project_Management.DTOs.TeamsDTOs;
using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Microsoft.AspNetCore.Identity;
using Repository.Identity;
using System.Text.Json;
using Domain.Enums;

namespace Graduation_Project_Management.Utilities
{
    public class SeedData
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public SeedData( UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async System.Threading.Tasks.Task SeedAsync()
        {
            #region Seed Roles
            string[] roles = { "Admin", "Student", "Supervisor" };
            foreach ( var role in roles )
            {
                if ( !await _roleManager.RoleExistsAsync(role) )
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            #endregion

            #region Seed Users
            var userJson = await File.ReadAllTextAsync("Seeding/users.json");
            var users = JsonSerializer.Deserialize<List<UserDto>>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            foreach ( var user in users )
            {
                var appUser = await _userManager.FindByEmailAsync(user.Email);
                if ( appUser == null )
                {
                    appUser = new AppUser
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.Email.Split('@')[0],
                        Email = user.Email
                    };
                    var result = await _userManager.CreateAsync(appUser, "Pa$$w0rd");
                    if ( result.Succeeded )
                    {
                        await _userManager.AddToRoleAsync(appUser, user.Role);
                    }
                }
            }
            #endregion

            #region Seed Supervisors
            var supervisorJson = await File.ReadAllTextAsync("Seeding/supervisors.json");
            var supervisors = JsonSerializer.Deserialize<List<SupervisorDto>>(supervisorJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            foreach ( var supervisor in supervisors )
            {
                var user = await _userManager.FindByEmailAsync(supervisor.Email);
                if ( user != null && !_context.Supervisors.Any(s => s.UserId == user.Id) )
                {
                    _context.Supervisors.Add(new Supervisor
                    {
                        FirstName = supervisor.FirstName,
                        LastName = supervisor.LastName,
                        Email = supervisor.Email,
                        UserId = user.Id,
                        PhoneNumber = supervisor.PhoneNumber,
                        Department = supervisor.Department,
                        ProfilePictureUrl = supervisor.ProfilePictureUrl,
                        MaxAssignedTeams = supervisor.MaxAssignedTeams,
                        PreferredTechnologies = supervisor.PreferredTechnologies
                    });
                }
            }
            await _context.SaveChangesAsync();
            #endregion

            #region Seed Students
            var studentJson = await File.ReadAllTextAsync("Seeding/students.json");
            var students = JsonSerializer.Deserialize<List<StudentDto>>(studentJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            foreach ( var student in students )
            {
                var user = await _userManager.FindByEmailAsync(student.Email);
                if ( user != null && !_context.Students.Any(s => s.UserId == user.Id) )
                {
                    _context.Students.Add(new Student
                    {
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        Email = student.Email,
                        UserId = user.Id,
                        PhoneNumber = student.PhoneNumber,
                        Department = student.Department,
                        ProfilePictureUrl = student.ProfilePictureUrl,
                        Gpa = student.Gpa,
                        TechStack = student.TechStack,
                        GithubProfile = student.GithubProfile,
                        LinkedInProfile = student.LinkedInProfile,
                        MainRole = student.MainRole,
                        SecondaryRole = student.SecondaryRole
                    });
                }
            }
            await _context.SaveChangesAsync();
            #endregion

            #region Seed Teams
            var teamJson = await File.ReadAllTextAsync("Seeding/teams.json");
            var teamDtos = JsonSerializer.Deserialize<List<TeamSeedDto>>(teamJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            foreach ( var teamDto in teamDtos )
            {
                if ( !_context.Teams.Any(t => t.Name == teamDto.Name) )
                {
                    var team = new Team
                    {
                        Name = teamDto.Name,
                        TeamDepartment = teamDto.TeamDepartment,
                        TechStack = teamDto.TechStack
                    };
                    _context.Teams.Add(team);
                }
            }
            await _context.SaveChangesAsync();
            #endregion

            #region Assign Students to Teams
            var teams = _context.Teams.ToList();
            foreach ( var teamDto in teamDtos )
            {
                var team = teams.FirstOrDefault(t => t.Name == teamDto.Name);
                if ( team != null )
                {
                    var members = _context.Students
                        .Where(s => teamDto.MembersEmails != null && teamDto.MembersEmails.Contains(s.Email))
                        .ToList();

                    foreach ( var member in members )
                    {
                        member.TeamId = team.Id;
                    }
                    team.TeamMembers = members;
                }
            }
            await _context.SaveChangesAsync();
            #endregion

            #region Seed ProjectIdeas
            var projectIdeaJson = await File.ReadAllTextAsync("Seeding/projectideas.json");
            var projectIdeaDtos = JsonSerializer.Deserialize<List<ProjectIdeaSeedDto>>(projectIdeaJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            foreach ( var dto in projectIdeaDtos )
            {
                if ( !_context.ProjectIdeas.Any(p => p.Title == dto.Title) )
                {
                    var supervisor = _context.Supervisors.FirstOrDefault(s => s.Email == dto.SupervisorEmail);
                    if ( supervisor == null ) continue;

                    _context.ProjectIdeas.Add(new ProjectIdea
                    {
                        Title = dto.Title,
                        Description = dto.Description,
                        TechStack = dto.TechStack,
                        CreatedAt = dto.CreatedAt,
                        UpdatedAt = dto.UpdaterdAt,
                        Status = Enum.Parse<ProjectIdeaStatus>(dto.Status),
                        TeamId = dto.TeamId,
                        SupervisorId = supervisor.Id,
                        IsCompleted = dto.IsCompleted,
                        CompletedAt = dto.CompletedAt
                    });
                }
            }
            await _context.SaveChangesAsync();
            #endregion

            #region Seed Tasks
            var taskJson = await File.ReadAllTextAsync("Seeding/tasks.json");
            var tasks = JsonSerializer.Deserialize<List<TaskSeedDto>>(taskJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            foreach ( var task in tasks )
            {
                if ( !_context.Tasks.Any(t => t.Title == task.Title) )
                {
                    var student = _context.Students.FirstOrDefault(s => s.Email == task.AssignedToEmail);
                    if ( student == null ) continue;

                    _context.Tasks.Add(new Domain.Entities.Task
                    {
                        Title = task.Title,
                        Description = task.Description,
                        Deadline = task.Deadline,
                        CreatedAt = task.CreatedAt,
                        Status = task.Status,
                        SupervisorId = task.SupervisorId,
                        TeamId = task.TeamId,
                        AssignedStudentId = student.Id
                    });
                }
            }
            #endregion

            await _context.SaveChangesAsync();
        }
    }
}