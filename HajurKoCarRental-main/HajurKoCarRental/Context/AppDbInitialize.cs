using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Identity;

namespace HajurKoCarRental.Context
{
    public class AppDbInitialize
    {
        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                // Roles
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                //if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Staff));
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));


                // Users
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

                var adminUserEmail = "admin@gmail.com";
                var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
                if (adminUser == null)
                {
                    var newAdminUser = new User
                    {
                        Name = "Admin",
                        UserName = "admin",
                        Email = adminUserEmail,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newAdminUser, "admin");
                    await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
                }
            }
        }
    }
}
