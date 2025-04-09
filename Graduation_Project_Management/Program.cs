using Domain.Entities.Identity;
using Graduation_Project_Management.Extension;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;

namespace Graduation_Project_Management
{
    public class Program
    {
        public static async Task Main( string[] args )
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Services

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // IdentityDatasbase Connection
            builder.Services.AddDbContext<AppIdentityContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("IdentitySQLConnection"));
            });

            builder.Services.ApplicationServices();
            builder.Services.AddIdentityService(builder.Configuration);

            #endregion Services

            var app = builder.Build();

            #region Update Database

            // auto migrate the database

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var Dbcontext = services.GetRequiredService<AppIdentityContext>();
                await Dbcontext.Database.MigrateAsync();

                var UserManger = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await AppIdentityDbContextSeed.SeedUserAsync(UserManger, roleManager);
            }
            catch ( Exception ex )
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "an error occured during appling the migration");
            }

            #endregion Update Database

            // Configure the HTTP request pipeline.
            if ( app.Environment.IsDevelopment() )
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}