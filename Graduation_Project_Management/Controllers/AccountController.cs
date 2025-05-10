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
using System.Security.Claims;

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
        private readonly IUnitOfWork _unitOfWork;

        public AccountController( UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IUnitOfWork unitOfWork )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
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
            if ( !result.Succeeded ) return BadRequest(result.Errors);

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

            await _unitOfWork.GetRepository<Student>().AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            // Get StudentId
            var studentEntity = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .FirstOrDefaultAsync(s => s.UserId == User.Id);
            if ( studentEntity == null )
                return BadRequest("Failed to create student profile.");

            var returnedUser = new UserDto()
            {
                UserId = studentEntity.Id,
                Role = "Student",
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
            if ( user is null ) return Unauthorized();

            var Result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if ( !Result.Succeeded ) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            if ( !roles.Contains("Student") ) return Unauthorized("Only students can log in here.");

            // Get StudentId
            var studentEntity = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
            if ( studentEntity == null )
                return BadRequest("Student profile not found.");

            var returnedUser = new UserDto()
            {
                UserId = studentEntity.Id,
                Role = "Student",
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