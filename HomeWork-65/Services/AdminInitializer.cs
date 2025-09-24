using HomeWork_65.Models;
using Microsoft.AspNetCore.Identity;

namespace HomeWork_65.Services;

public class AdminInitializer
{
    public static async Task SeedAdminUser(RoleManager<IdentityRole<int>> roleManager,
        UserManager<MyUser> userManager)
    {
        string adminEmail = "admin@admin.com";
        string adminPassword = "1qwe@QWE";
        var roles = new[] { "admin", "user" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                MyUser admin = new MyUser { Email = adminEmail, UserName = adminEmail, };
                IdentityResult result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}