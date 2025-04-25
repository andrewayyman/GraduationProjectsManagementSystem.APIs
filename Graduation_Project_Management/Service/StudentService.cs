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

namespace Graduation_Project_Management.Service
{
    public class StudentService :IStudentService
    {
        #region Dependencies
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.SaveChangesAsync();

            return new OkObjectResult(new { message = "Profile updated successfully" });
        } 
        #endregion
    }
}
