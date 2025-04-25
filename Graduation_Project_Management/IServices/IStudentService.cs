using Graduation_Project_Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.IServices
{
    public interface IStudentService
    {
       
       
        Task<IActionResult> UpdateStudentProfileAsync(ClaimsPrincipal user, UpdateStudentProfileDto dto);
    }
}

