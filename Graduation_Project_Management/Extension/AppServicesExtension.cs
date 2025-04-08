using Domain.Repository;
using Repository;

namespace Graduation_Project_Management.Extension
{
    public static class AppServicesExtension
    {
        public static IServiceCollection ApplicationServices(this IServiceCollection Services)
        {
            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            return Services;
        }
    }
}
