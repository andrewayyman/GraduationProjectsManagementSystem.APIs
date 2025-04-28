using Domain.Entities.Identity;
using Domain.Entities;
using Domain.Repository;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.Errors;

namespace Graduation_Project_Management.Service
{
    public class SupervisorService : ISupervisorService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Supervisor> _supervisorRepo;
        private readonly IUnitOfWork _unitOfWork;

        public SupervisorService( UserManager<AppUser> userManager, ApplicationDbContext context, IGenericRepository<Supervisor> supervisorRepo, IUnitOfWork unitOfWork )
        {
            _userManager = userManager;
            _context = context;
            _supervisorRepo = supervisorRepo;
            _unitOfWork = unitOfWork;
        }

        #region GettAllSupervisors Service

        public async Task<ActionResult> GetAllSupervisorsAsync()
        {
            var supervisors = await _unitOfWork.GetRepository<Supervisor>().GetAllAsync().ToListAsync();
            if ( supervisors == null || !supervisors.Any() )
                return new NotFoundObjectResult(new ApiResponse(404, "There are no supervisors."));

            var ReturnedSupervisors = supervisors.Select(s => new SupervisorDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Department = s.Department,
                ProfilePictureUrl = s.ProfilePictureUrl,
                MaxAssignedTeams = s.MaxAssignedTeams,
                PreferredTechnologies = s.PreferredTechnologies,
                SupervisedTeams = _context.Teams
                    .Where(t => t.SupervisorId == s.Id)
                    .Select(t => new TeamDto
                    {
                        Name = t.Name,
                        TechStack = t.TechStack
                    }).ToList(),
            }).ToList();

            return new OkObjectResult(ReturnedSupervisors);
        }

        #endregion GettAllSupervisors Service
    }
}