using Domain.Entities.Identity;
using Domain.Entities;
using Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repository.Identity;
using Microsoft.EntityFrameworkCore;
using Graduation_Project_Management.DTOs.SupervisorDTOs;
using Graduation_Project_Management.Errors;
using System.Security.Claims;
using Domain.Enums;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.IServices;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorController : ControllerBase
    {
        private readonly ISupervisorService _supervisorService;

        public SupervisorController( ISupervisorService supervisorService )
        {
            _supervisorService = supervisorService;
    
        }

        #region GetAllSupervisors

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupervisorDto>>> GetAll()
            => await _supervisorService.GetAllSupervisorsAsync();

        #endregion GetAllSupervisors

        #region GetSupervisorById

        [HttpGet("{id}")]
        public async Task<ActionResult<SupervisorDto>> GetById( int id )
            => await _supervisorService.GetSupervisorByIdAsync(id);

        #endregion GetSupervisorById

        #region UpdateSupervisor

        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateSupervisorDto>> Update( int id, [FromBody] UpdateSupervisorDto supervisorDto )
            => await _supervisorService.UpdateSupervisorProfileAsync(id, supervisorDto);

        #endregion UpdateSupervisor

        #region DeleteSupervisor

        [HttpDelete("{id}")]
        public async Task<ActionResult<SupervisorDto>> Delete( int id )
            => await _supervisorService.DeleteSupervisorProfileAsync(id);

        #endregion DeleteSupervisor

        #region GetPendingRequests

        // need to be updated that once request approved remove it from here

        [HttpGet("GetPendingRequests")]
        public async Task<IActionResult> GetPendingRequests()
            => await _supervisorService.GetPendingRequestsAsync(User);

        #endregion GetPendingRequests

        #region GetMyTeams

        [HttpGet("GetMyTeams")]
        public async Task<IActionResult> GetMyTeams()
            => await _supervisorService.GetMyTeamsAsync(User);

        #endregion GetMyTeams

        #region HandleRequest

        [HttpPut("HandleRequest")]
        public async Task<IActionResult> HandleRequest( [FromBody] HandleIdeaRequestDto dto )
            => await _supervisorService.HandleIdeaRequestAsync(User, dto);

        #endregion HandleRequest
    }
}