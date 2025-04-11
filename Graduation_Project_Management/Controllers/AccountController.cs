using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Services;
using Graduation_Project_Management.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region Dependencies

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _appIdentityContext;

        public AccountController( UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, ApplicationDbContext appIdentityContext )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _appIdentityContext = appIdentityContext;
        }

        #endregion Dependencies

        #region Register

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register( RegisterDto model )
        {
            var User = new AppUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email.Split('@')[0]
            };
            var result = await _userManager.CreateAsync(User, model.Password);
            if ( !result.Succeeded ) return BadRequest(result.Errors);//(new Apiresponse(400))

            await _userManager.AddToRoleAsync(User, "Student");

            // Create Student entry
            var student = new Student
            {
                UserId = User.Id,
                FirstName = User.FirstName,
                LastName = User.LastName,
                Email = User.Email,
                PhoneNumber = User.PhoneNumber
            };
            _appIdentityContext.Students.Add(student);
            await _appIdentityContext.SaveChangesAsync();

            var returnedUser = new UserDto()
            {
                Email = User.Email,
                FirstName = User.FirstName,
                LastName = User.LastName,
                Token = await _tokenService.CreateTokenAsync(User, _userManager)
            };
            return Ok(returnedUser);
        }

        #endregion Register

        #region Login

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login( LoginDto model )
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if ( user is null ) return Unauthorized();//(new ApiResponse(401))
            var Result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if ( !Result.Succeeded ) return Unauthorized();//(new ApiResponse(401))
            var returnedUser = new UserDto()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
            return Ok(returnedUser);
        }

        #endregion Login

       
        


    }
}