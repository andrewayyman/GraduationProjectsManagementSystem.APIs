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
                    DisplayName = "Soma Ali",
                    Email = "Somaali@Helwan.com",
                    UserName = "smsoma",
                    PhoneNumber = "01111111111"
                };

                await userManager.CreateAsync(User, "Pa$$wOrd");
                await userManager.AddToRoleAsync(User, "Admin");
            }
        }
    }
}