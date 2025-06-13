using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUserAsync( UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager )
        {
            if ( !roleManager.Roles.Any() )
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("Student"));
                await roleManager.CreateAsync(new IdentityRole("Supervisor"));
            }

            if ( !userManager.Users.Any() )
            {
                var User = new AppUser()
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@fci.helwan.edu.eg",
                    UserName = "Admin",
                    PhoneNumber = "01206741192"
                };

                await userManager.CreateAsync(User, "Pa$$w0rd");
                await userManager.AddToRoleAsync(User, "Admin");
            }
        }
    }
}