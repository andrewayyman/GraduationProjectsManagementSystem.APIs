using Graduation_Project_Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IStudentService
    {
        Task<IActionResult> DeleteStudentProfileAsync(int id);
        Task<IActionResult> GetAllStudentsAsync();
        Task<IActionResult> GetStudentByIdAsync(int id);
        Task<IActionResult> UpdateStudentProfileAsync(ClaimsPrincipal user, UpdateStudentProfileDto dto);
    }
}

