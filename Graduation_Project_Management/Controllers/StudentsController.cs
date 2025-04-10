using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Repository;
using Graduation_Project_Management.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        #region Dependencies

        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<Student> _studentRepo;

        public StudentsController(

            UserManager<AppUser> userManager,
            IGenericRepository<Student> studentRepo

            )
        {
            _userManager = userManager;
            _studentRepo = studentRepo;
        }

        #endregion Dependencies
    }
}