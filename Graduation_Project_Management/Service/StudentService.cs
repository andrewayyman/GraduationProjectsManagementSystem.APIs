using Domain.Entities.Identity;
using Domain.Entities;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Graduation_Project_Management.Utilities;
using Graduation_Project_Management.Errors;

namespace Graduation_Project_Management.Service
{
    public class StudentService : IStudentService
    {
        #region Dependencies

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public StudentService( IUnitOfWork unitOfWork, UserManager<AppUser> userManager )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        #endregion Dependencies


        #region GetAllStudents

        public async Task<IActionResult> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .Include(s=>s.Team)
                .ToListAsync();
            var studentDtos = students.Select(student => new StudentDto
            {
                StudentId = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Department = student.Department,
                Gpa = student.Gpa,
                TechStack = student.TechStack,
                GithubProfile = student.GithubProfile,
                LinkedInProfile = student.LinkedInProfile,
                MainRole = student.MainRole,
                SecondaryRole = student.SecondaryRole,
                ProfilePictureUrl = student.ProfilePictureUrl,
                TeamId = student.Team?.Id,
                TeamName = student.Team?.Name
            }).ToList();
            return new OkObjectResult(studentDtos);
        }

        #endregion Get All

        #region GetStudentById

        public async Task<IActionResult> GetStudentByIdAsync( int id ,ClaimsPrincipal user)
        {
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                        .Include(s => s.Team)
                        .FirstOrDefaultAsync(s => s.Id == id);

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404 , "Student Not Found"));
            
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if ( student.Email != userEmail && !roles.Contains("Admin") )
                return new UnauthorizedObjectResult(new ApiResponse(403, "You are not authorized to view this profile"));


            var studentDto = new StudentDto
            {
                StudentId = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Department = student.Department,
                Gpa = student.Gpa,
                TechStack = student.TechStack,
                GithubProfile = student.GithubProfile,
                LinkedInProfile = student.LinkedInProfile,
                MainRole = student.MainRole,
                SecondaryRole = student.SecondaryRole,
                ProfilePictureUrl = student.ProfilePictureUrl,
                TeamId = student.Team?.Id ,
                TeamName = student.Team.Name
            };

            return new OkObjectResult(studentDto);
        }

        #endregion Get  By Id

        #region UpdateStudent

        public async Task<IActionResult> UpdateStudentProfileAsync( int studentId, UpdateStudentProfileDto dto, ClaimsPrincipal user )
        {
            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(studentId);

            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404 , "Student Not Found"));
           
            var userEmail = user.FindFirstValue(ClaimTypes.Email);

            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if ( student.Email != userEmail && !roles.Contains("Admin") )
                return new ObjectResult(new ApiResponse(403, "Unauthorized to update this student."));


            student.FirstName = dto.FirstName ?? student.FirstName;
            student.LastName = dto.LastName ?? student.LastName;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Department = dto.Department ?? student.Department;
            student.Gpa = dto.Gpa ?? student.Gpa;
            student.TechStack = dto.TechStack ?? student.TechStack;
            student.GithubProfile = dto.GithubProfile ?? student.GithubProfile;
            student.LinkedInProfile = dto.LinkedInProfile ?? student.LinkedInProfile;
            student.MainRole = dto.MainRole ?? student.MainRole;
            student.SecondaryRole = dto.SecondaryRole ?? student.SecondaryRole;


            if ( dto.ProfilePictureUrl != null )
            {
                if ( !string.IsNullOrEmpty(student.ProfilePictureUrl) )
                {
                    DocumentSetting.DeleteFile("StudentPictures", student.ProfilePictureUrl);
                }

                var fileName = await DocumentSetting.UploadFileAsync(dto.ProfilePictureUrl, "StudentPictures");
                student.ProfilePictureUrl = fileName;
            }

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new ApiResponse(200 , "Profile updated successfully"));
        }

        #endregion Update Student Profile

        #region DeleteStudent

        public async Task<IActionResult> DeleteStudentProfileAsync( int studentId, ClaimsPrincipal user )
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var studentRepo = _unitOfWork.GetRepository<Student>();
            var student = await studentRepo.GetAllAsync()
                .Where(s => s.Id == studentId)
                .Include(s => s.Team)
                .Include(s => s.JoinRequests)
                .FirstOrDefaultAsync();


            if ( student == null )
                return new NotFoundObjectResult(new ApiResponse(404 , "Student Not FOund"));

            // Allow only the Student themselves or Admin
            if ( student.Email != userEmail && !roles.Contains("Admin") )
                return new ObjectResult( new ApiResponse(403 , "Unauthorized to delete this student.") ) ;

            var appUser = await _userManager.FindByEmailAsync(student.Email);
            if ( appUser == null )
                return new NotFoundObjectResult ( new ApiResponse(404 , "User not found") );

            if ( student.Team != null )
            {
                student.Team.TeamMembers.Remove(student);
            }

            if ( student.JoinRequests != null && student.JoinRequests.Any() )
            {
                var joinRequestRepo = _unitOfWork.GetRepository<TeamJoinRequest>();
                foreach ( var request in student.JoinRequests )
                {
                    await joinRequestRepo.DeleteAsync(request);
                }
            }

            await studentRepo.DeleteAsync(student);
            var result = await _userManager.DeleteAsync(appUser);
            if ( !result.Succeeded )
                return new BadRequestObjectResult(new ApiResponse(404 , "Failed to delete user from Identity."));

            await _unitOfWork.SaveChangesAsync();
            return new OkObjectResult(new ApiResponse(200 , "Student and user deleted successfully"));
        }
        #endregion Delete


    }
}