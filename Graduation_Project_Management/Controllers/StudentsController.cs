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

        public StudentsController( IStudentService studentService )
        {
            _studentService = studentService;
        }

        #endregion Dependencies

        #region Get All

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllStudents()
        {
            return await _studentService.GetAllStudentsAsync();
        }

        #endregion Get All

        #region Get  By Id

        [HttpGet("{id}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> GetById( int id )
        {
            return await _studentService.GetStudentByIdAsync(id, User);
        }

        #endregion Get  By Id

        #region Update

        [HttpPut("{id}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> Update( int id, [FromForm] UpdateStudentProfileDto dto )
        {
            return await _studentService.UpdateStudentProfileAsync(id, dto, User);
        }

        #endregion Update

        #region Delete

        [HttpDelete("{id}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> Delete( int id )
        {
            return await _studentService.DeleteStudentProfileAsync(id, User);
        }

        #endregion Delete

      

    }
}