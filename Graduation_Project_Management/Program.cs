using Domain.Entities.Identity;
using Graduation_Project_Management.Extension;
using Graduation_Project_Management.Hubs;
using Graduation_Project_Management.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;

namespace Graduation_Project_Management
{
    public class Program
    {
        public static async Task Main( string[] args )
        {
            #region Services

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // IdentityDatasbase Connection
            builder.Services.AddDbContext<ApplicationDbContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("IdentitySQLConnection"));
            });

            builder.Services.ApplicationServices(builder.Configuration);
            builder.Services.AddIdentityService(builder.Configuration);
            builder.Services.AddSignalR();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddHttpClient();

            #endregion Services

            var app = builder.Build();


            #region Update Database

            // auto migrate the database

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var Dbcontext = services.GetRequiredService<ApplicationDbContext>();
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

            #region Middlewares


            // Configure CORS


            // Configure the HTTP request pipeline.
            if ( app.Environment.IsDevelopment() )
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowFrontend");


            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
             {
                 endpoints.MapHub<ChatHub>("/hubs/chatHub");
                 endpoints.MapHub<NotificationHub>("/hubs/notificationHub");
                 endpoints.MapControllers();
             });

            app.MapControllers();

            // Seed Data
            using var seedScope = app.Services.CreateScope();
            var seedServices = seedScope.ServiceProvider;
            var seedData = seedServices.GetRequiredService<SeedData>();
            await seedData.SeedAsync();

            #endregion Middlewares

            app.Run();
        }
    }
}