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

namespace Graduation_Project_Management.Service
{
    public class StudentService :IStudentService
    {
        #region Dependencies
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public StudentService(IUnitOfWork unitOfWork,UserManager<AppUser> userManager )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        } 
        #endregion

        #region Update Student Profile 
        public async Task<IActionResult> UpdateStudentProfileAsync(ClaimsPrincipal user, UpdateStudentProfileDto dto)
        {
            var userEmail = user.FindFirstValue(ClaimTypes.Email);
            var student = await _unitOfWork.GetRepository<Student>().GetAllAsync()
                .FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student == null)
                return new NotFoundObjectResult("Student not found");

            student.FirstName = dto.FirstName ?? student.FirstName;
            student.LastName = dto.LastName ?? student.LastName;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Department = dto.Department ?? student.Department;
            student.Gpa = dto.Gpa ?? student.Gpa;
            student.TechStack = dto.TechStack ?? student.TechStack;
            student.GithubProfile = dto.GitHubProfile ?? student.GithubProfile;
            student.LinkedInProfile = dto.LinkedInProfile ?? student.LinkedInProfile;
            student.MainRole = dto.MainRole ?? student.MainRole;
            student.SecondaryRole = dto.SecondaryRole ?? student.SecondaryRole;
            if (dto.ProfilePictureUrl != null)
            {
                // لو فيه صورة قديمة احذفها
                if (!string.IsNullOrEmpty(student.ProfilePictureUrl))
                {
                    DocumentSetting.DeleteFile("StudentPictures", student.ProfilePictureUrl);
                }

                var fileName = await DocumentSetting.UploadFileAsync(dto.ProfilePictureUrl, "StudentPictures");
                student.ProfilePictureUrl = fileName;
            }

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new { message = "Profile updated successfully" });
        }
        #endregion

        #region Delete 
        public async Task<IActionResult> DeleteStudentProfileAsync(int studentId)
        {
            var studentRepo = _unitOfWork.GetRepository<Student>();
            var student = await studentRepo.GetAllAsync()
                .Include(s => s.Team)
                .Include(s => s.JoinRequests)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return new NotFoundObjectResult("Student not found");

            // نحضر اليوزر بالاميل
            var user = await _userManager.FindByEmailAsync(student.Email);

            if (user == null)
                return new NotFoundObjectResult("User not found");

            if (student.Team != null)
            {
                student.Team.TeamMembers.Remove(student);
            }

            if (student.JoinRequests != null && student.JoinRequests.Any())
            {
                var joinRequestRepo = _unitOfWork.GetRepository<TeamJoinRequest>();
                foreach (var request in student.JoinRequests)
                {
                    await joinRequestRepo.DeleteAsync(request);
                }
            }

            await studentRepo.DeleteAsync(student);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new { message = "Failed to delete user from Identity." });
            }

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new { message = "Student and user deleted successfully" });
        }

        #endregion

        #region Get All 
        public async Task<IActionResult> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.GetRepository<Student>().GetAllAsync().ToListAsync();
            var studentDtos = students.Select(student => new StudentDto
            {
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
                SecondaryRole = student.SecondaryRole
            }).ToList();
            return new OkObjectResult(studentDtos);
        }
        #endregion

        #region Get  By Id
        public async Task<IActionResult> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(id);

            if (student == null)
                return new NotFoundObjectResult("Student not found");
            var studentDto = new StudentDto
            {
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
                SecondaryRole = student.SecondaryRole
            };

            return new OkObjectResult(studentDto);
        }
        #endregion
    }
}
