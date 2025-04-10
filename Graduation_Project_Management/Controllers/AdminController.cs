using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Services;
using Graduation_Project_Management.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ApplicationDbContext _appIdentityContext;
        private readonly ITokenService _tokenService;

        public AdminController( UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext appIdentityContext, ITokenService tokenService )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _appIdentityContext = appIdentityContext;
            _tokenService = tokenService;
        }

        #endregion Dependencies

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

            _appIdentityContext.Supervisors.Add(supervisor);
            await _appIdentityContext.SaveChangesAsync();

            var returnedUser = new UserDto()
            {
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