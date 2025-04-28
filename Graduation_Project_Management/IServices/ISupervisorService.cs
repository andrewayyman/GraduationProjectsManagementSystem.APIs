using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project_Management.IServices
{
    public interface ISupervisorService
    {
        Task<ActionResult> GetAllSupervisorsAsync();

        //Task<IActionResult> GetSupervisorById( int id );
    }
}