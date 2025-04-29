using Graduation_Project_Management.IServices;
using Graduation_Project_Management.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Graduation_Project_Management.IServices;

using Graduation_Project_Management.DTOs.TasksDTOs;
using AutoMapper;
using Graduation_Project_Management.Errors;

namespace Graduation_Project_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITasksServices _tasksServices;

        public TasksController( ITasksServices tasksServices )
        {
            _tasksServices = tasksServices;
        }

        #region CreateTask

        [HttpPost]
        public async Task<ActionResult> CreateTask( [FromBody] CreateTaskDto dto )
        {
            try
            {
                var result = await _tasksServices.CreateTaskAsync(dto, User);

                if ( result is OkObjectResult okResult )
                {
                    return Ok(okResult.Value);
                }

                if ( result is NotFoundObjectResult notFoundResult )
                {
                    return NotFound(notFoundResult.Value);
                }

                return BadRequest(new ApiResponse(400, "Invalid request"));
            }

            catch ( UnauthorizedAccessException ex )
            {
                return StatusCode(500, new ApiResponse(500, "An error occurred while creating the task"));
            }
        }

        #endregion CreateTask
    }
}