using Graduation_Project_Management.DTOs.ProjectIdeasDTOs;
using Graduation_Project_Management.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectIdeaController : ControllerBase
    {
        #region  Dependencies

        private readonly IProjectIdeaService _projectIdeaService;

        public ProjectIdeaController(IProjectIdeaService projectIdeaService)
        {
            _projectIdeaService = projectIdeaService;
        }

        #endregion  Dependencies

        #region Publish Project Idea

        [HttpPost("PublishIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> PublishProjectIdea([FromBody] PublishProjectIdeaDto dto)
        => await _projectIdeaService.PublishProjectIdeaAsync(User, dto);

        #endregion Publish Project Idea

        #region Delete Idea 
        [HttpDelete("DeleteIdea/{ideaId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteIdea(int ideaId)
        => await _projectIdeaService.DeleteIdeaAsync(User, ideaId);


        #endregion

        #region  Update Idea
        [HttpPut("UpdateIdea/{ideaId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateIdea(int ideaId, [FromBody] PublishProjectIdeaDto dto)
       => await _projectIdeaService.UpdateIdeaAsync(User, ideaId, dto);
        #endregion
    }
}
