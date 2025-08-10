using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // if your DbContext derives from IdentityDbContext<User>

namespace UptimeMonitor.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // 1) Destructive clear (order matters: dependents first)
            context.ScheduledChecks.RemoveRange(context.ScheduledChecks);
            await context.SaveChangesAsync();

            // Clear Identity tables in safe order
            context.UserClaims.RemoveRange(context.UserClaims);
            context.UserLogins.RemoveRange(context.UserLogins);
            context.UserRoles.RemoveRange(context.UserRoles);
            context.UserTokens.RemoveRange(context.UserTokens);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            // 2) Create fresh users via UserManager (proper hashing etc.)
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var admin = new User
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                Firstname = "Admin",
                Lastname = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");

            var user = new User
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                Firstname = "Regular",
                Lastname = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, "User123!");

            // IMPORTANT: re-query users so they’re tracked by *this* context
            var adminTracked = await context.Users.SingleAsync(u => u.Email == "admin@example.com");
            var userTracked  = await context.Users.SingleAsync(u => u.Email == "user@example.com");

            // 3) Seed domain data
            context.ScheduledChecks.AddRange(
                new ScheduledCheck
                {
                    Domain = "https://example.com",
                    Users = new List<User> { adminTracked }
                },
                new ScheduledCheck
                {
                    Domain = "https://example.org",
                    Users = new List<User> { userTracked }
                },
                new ScheduledCheck
                {
                    Domain = "https://example.net",
                    Users = new List<User> { adminTracked, userTracked }
                });

            await context.SaveChangesAsync();
        }
    }
}
