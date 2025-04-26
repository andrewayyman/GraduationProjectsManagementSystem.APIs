using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.Repository;
using Graduation_Project_Management.DTOs;
using Graduation_Project_Management.IServices;
using Graduation_Project_Management.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System.Security.Claims;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase

    {
        #region  Dependencies

        private readonly ITeamService _teamService;

        public TeamController( ITeamService teamService )
        {
            _teamService = teamService;
        }

        #endregion  Dependencies     

        #region Create team

        [HttpPost("Create")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateTeam([FromBody] TeamDto model)
        {
            return await _teamService.CreateTeamAsync(User, model);
        }

        #endregion Create team

        #region Delete Team

        [HttpDelete("{teamId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            return await _teamService.DeleteTeamAsync(User, teamId);
        }

        #endregion Delete Team

        #region Get All

        [HttpGet("Available")]
        public async Task<IActionResult> GetAvailableTeams()
        {
            return await _teamService.GetAvailableTeamsAsync();
        }

        #endregion Get All Available Teams

        #region Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return await _teamService.GetTeamByIdAsync(id);
        } 
        #endregion

        #region Update
        [HttpPut("Update")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateTeamProfile([FromBody] UpdateTeamDto dto)
        {
            return await _teamService.UpdateTeamProfileAsync(User, dto);
        }

        #endregion
    }
}