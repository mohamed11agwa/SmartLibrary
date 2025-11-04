using Microsoft.AspNetCore.Identity;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;

namespace SmartLibrary.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new ApplicationUser()
            {
                UserName = "admin@smartlibrary.com",
                Email = "admin@smartlibrary.com",
                FullName = "Admin",
                EmailConfirmed = true
            };
      
            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user is null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }

        }








    }
}
