using Graduation_Project_Management.DTOs.StudentDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IStudentService
    {
        Task<IActionResult> GetAllStudentsAsync();
        Task<IActionResult> GetStudentByIdAsync(int id , ClaimsPrincipal user);
        Task<IActionResult> UpdateStudentProfileAsync( int studentId, UpdateStudentProfileDto dto, ClaimsPrincipal user );
        Task<IActionResult> DeleteStudentProfileAsync( int studentId, ClaimsPrincipal user ); 
    }
}

