using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { GlobalConstants.GuestUser, GlobalConstants.RoleUser, GlobalConstants.AdminUser };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var newRole = new ApplicationRole()
                    {
                        Name = roleName,
                        Status = Status.Enabled,
                        CreatedDate = DateTime.Now,
                        LastUpdatedDate = DateTime.UtcNow,
                    }; 
                    roleResult = await roleManager.CreateAsync(newRole);
                }
            }

            var admin = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                Status = Status.Enabled,
                CreatedDate = DateTime.Now,
                LastUpdatedDate = DateTime.UtcNow,
                EmailConfirmed = true,
            };

            string adminPassword = "Admin123!";
            var _user = await userManager.FindByEmailAsync("admin@admin.com");

            if (_user == null)
            {
                var createAdmin = await userManager.CreateAsync(admin, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, GlobalConstants.AdminUser);
                }
            }

           

            


        }
    
    }
}
