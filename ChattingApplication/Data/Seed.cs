using ChattingApplication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChattingApplication.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var roles = new List<AppRole>()
            {
                new AppRole{Name = "Member" },
                new AppRole{Name = "Admin" },
                new AppRole{Name = "Moderator" },

            };
            foreach(var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            foreach(var user in users)
            {

                user.UserName = user.UserName.ToLower();
              
               await userManager.CreateAsync(user, "P@$$w0rd");
               await userManager.AddToRoleAsync(user, "Member");
            }
            var admin = new AppUser
            {
                UserName = "admin"
            };
            await userManager.CreateAsync(admin, "P@$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin" , "Moderator" });

        }
    }
}
