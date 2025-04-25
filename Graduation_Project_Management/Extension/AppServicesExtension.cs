using Domain.Repository;
using Graduation_Project_Management.IServices;
using Graduation_Project_Management.Service;
using Repository;

namespace Graduation_Project_Management.Extension
{
    public static class AppServicesExtension
    {
        public static IServiceCollection ApplicationServices(this IServiceCollection Services)
        {
            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            Services.AddScoped(typeof(IStudentService),typeof(StudentService));
            Services.AddScoped(typeof(ITeamService), typeof(TeamService));
            Services.AddScoped(typeof(IProjectIdeaService), typeof(ProjectIdeaService));
            Services.AddScoped(typeof(IRequestService), typeof(RequestService));


            return Services;
        }
    }
}
