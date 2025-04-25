using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.Errors;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        #region Dependencies     
        
        private readonly IStudentService _studentService;
        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        #endregion Dependencies

        #region Update Student Profile

        [HttpPut("UpdateProfile")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileDto dto)
        {
            return await _studentService.UpdateStudentProfileAsync(User, dto);
        }

        #endregion Update Student Profile
    }
}