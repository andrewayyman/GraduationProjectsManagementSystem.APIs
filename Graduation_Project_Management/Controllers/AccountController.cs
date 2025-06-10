using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Repository;
using Domain.Services;
using Graduation_Project_Management.DTOs.AuthDTOs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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

        private readonly IEmailSenderService _emailSender;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController( UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IUnitOfWork unitOfWork, IEmailSenderService emailSender )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
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


            // إرسال رابط تأكيد البريد
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(User);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new
            {
                userId = User.Id,
                token = token
            }, Request.Scheme);

            await _emailSender.SendEmailAsync(User.Email, "Confirm your email",
                $"<p>Please confirm your account by clicking <a href='{confirmationLink}'>here</a>.</p>");


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
            if ( user == null ) return Unauthorized("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault(); // Assume single role for simplicity

            //if (!user.EmailConfirmed && role!="Admin")
            //    return Unauthorized("Please confirm your email before logging in.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if ( !result.Succeeded ) return Unauthorized("Invalid email or password.");

           
            if ( role == null ) return BadRequest("User has no role assigned.");

            UserDto returnedUser;

            if ( role == "Student" )
            {
                var studentEntity = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
                if ( studentEntity == null )
                    return BadRequest("Student profile not found.");

                returnedUser = new UserDto
                {
                    UserId = studentEntity.Id,
                    Role = "Student",
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = await _tokenService.CreateTokenAsync(user, _userManager)
                };
            }
            else if ( role == "Supervisor" )
            {
                var supervisorEntity = await _unitOfWork.GetRepository<Supervisor>().GetAllAsync()
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
                if ( supervisorEntity == null )
                    return BadRequest("Supervisor profile not found.");

                returnedUser = new UserDto
                {
                    UserId = supervisorEntity.Id,
                    Role = "Supervisor",
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = await _tokenService.CreateTokenAsync(user, _userManager)
                };
            }
            else if ( role == "Admin" )
            {
                returnedUser = new UserDto
                {
                    //UserId = user.Id, // Use AppUser.Id for Admin
                    Role = "Admin",
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = await _tokenService.CreateTokenAsync(user, _userManager)
                };
            }
            else
            {
                return BadRequest("Invalid user role.");
            }

            return Ok(returnedUser);
        }

        #endregion Login

        #region ConfirmEmail

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("Invalid user ID.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return Ok("Email confirmed successfully.");

            return BadRequest("Email confirmation failed.");
        }
        #endregion

        #region ForgotPassword //frondend Url is needed 
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest("Invalid request.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var frontendResetPasswordUrl = $"https://myfrontend.com/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(user.Email, "Reset Password", $"Click here to reset your password: {frontendResetPasswordUrl}");

            return Ok("Password reset link has been sent to your email.");
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
                return Ok("Password has been reset successfully.");

            return BadRequest("Error while resetting the password.");
        } 
        #endregion


    }
}