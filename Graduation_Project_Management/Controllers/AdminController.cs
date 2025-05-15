using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Repository;
using Domain.Services;
using Graduation_Project_Management.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        #region Dependencies

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        public AdminController( UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService, IUnitOfWork unitOfWork )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }
        #endregion


        #region Register_Supervisor

        [Authorize(Roles = "Admin")]
        [HttpPost("RegisterSupervisor")]
        public async Task<ActionResult> RegisterSupervisor( RegisterSupervisorDto model )
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if ( existingUser != null )
                return BadRequest("Email is already registered.");

            var supervisorUser = new AppUser
            {
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
                FirstName = model.FirstName,
                LastName = model.LastName,
            };

            var result = await _userManager.CreateAsync(supervisorUser, model.Password);
            if ( !result.Succeeded )
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(supervisorUser, "Supervisor");

            var supervisor = new Supervisor
            {
                UserId = supervisorUser.Id,
                FirstName = supervisorUser.FirstName,
                LastName = supervisorUser.LastName,
                Email = supervisorUser.Email,
                // Add any additional props
            };

            await _unitOfWork.GetRepository<Supervisor>().AddAsync(supervisor);
            await _unitOfWork.SaveChangesAsync();

            // Get SupervisorId
            var supervisorEntity = await _unitOfWork.GetRepository<Supervisor>().GetAllAsync()
                .FirstOrDefaultAsync(s => s.UserId == supervisorUser.Id);
            if ( supervisorEntity == null )
                return BadRequest("Failed to create supervisor profile.");

            var returnedUser = new UserDto()
            {
                UserId = supervisorEntity.Id,
                Role = "Supervisor",
                Email = supervisorUser.Email,
                FirstName = supervisorUser.FirstName,
                LastName = supervisorUser.LastName,
                Token = await _tokenService.CreateTokenAsync(supervisorUser, _userManager)
            };
            return Ok(returnedUser);
        }

        #endregion Register_Supervisor
    }
}
