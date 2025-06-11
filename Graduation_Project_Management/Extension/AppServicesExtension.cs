using Domain.Entities;
using Domain.Repository;
using Graduation_Project_Management.IServices;
using Graduation_Project_Management.Service;
using Graduation_Project_Management.Utilities;
using Repository;

namespace Graduation_Project_Management.Extension
{
    public static class AppServicesExtension
    {
        public static IServiceCollection ApplicationServices( this IServiceCollection Services, IConfiguration _configuration)
        {
            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            Services.AddScoped(typeof(IStudentService), typeof(StudentService));
            Services.AddScoped(typeof(ITeamService), typeof(TeamService));
            Services.AddScoped(typeof(IProjectIdeaService), typeof(ProjectIdeaService));
            Services.AddScoped(typeof(IRequestService), typeof(RequestService));
            Services.AddScoped(typeof(ISupervisorService), typeof(SupervisorService));
            Services.AddScoped(typeof(ITasksServices), typeof(TasksServices));
            Services.AddScoped(typeof(IMeetingService), typeof(MeetingsServices));
            Services.AddScoped<SeedData>();

            Services.Configure<EmailSettings>(_configuration.GetSection("EmailSettings"));
            Services.AddTransient<IEmailSenderService, EmailSender>();


            return Services;
        }
    }
}