using Domain.Entities.Identity;
using Graduation_Project_Management.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController( UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager )
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Assign-Roles

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleToUser( AssignRoleDto model )
        {
            // Check if role exists
            if ( !await _roleManager.RoleExistsAsync(model.Role) )
                return BadRequest("Invalid role.");

            // Get user
            var user = await _userManager.FindByNameAsync(model.UserName);
            if ( user == null )
                return NotFound("User not found.");

            // Assign role
            var result = await _userManager.AddToRoleAsync(user, model.Role);

            if ( !result.Succeeded )
                return BadRequest(result.Errors);

            return Ok(new { message = $"Role '{model.Role}' assigned to user '{model.UserName}'" });
        }

        #endregion Assign-Roles
    }
}