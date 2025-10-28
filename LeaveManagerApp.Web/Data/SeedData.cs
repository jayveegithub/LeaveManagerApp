using LeaveManagerApp.Web.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LeaveManagerApp.Web.Domain.Models;

namespace LeaveManagerApp.Web.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();

			async Task<ApplicationUser> EnsureUser(string username, string email, string fullName)
			{
				var usr = await userManager.FindByEmailAsync(email);
				if (usr != null) return usr;
				usr = new ApplicationUser { UserName = username, Email = email, FullName = fullName, IsActive = true, EmailConfirmed = true };
				var r = await userManager.CreateAsync(usr, "@User456!");
				if (!r.Succeeded) throw new Exception("Unable to create user: " + string.Join(", ", r.Errors.Select(e => e.Description)));
				return usr;
			}

			// Requirement: Ensure Manager is created and active
			var managerRole = "Manager";
			if (!await roleManager.RoleExistsAsync(managerRole))
				await roleManager.CreateAsync(new IdentityRole(managerRole));
            // Create Managers
			var manager1 = await EnsureUser("sandra", "sandra@mycompany.com", "Sandra Manager");
			var manager2 = await EnsureUser("paul", "paul@mycompany.com", "Paul Manager");
			await userManager.AddToRoleAsync(manager1, managerRole);
			await userManager.AddToRoleAsync(manager2, managerRole);

			// Create Employees
			await EnsureUser("mike", "mike@mycompany.com", "Mike Simmons");
            await EnsureUser("leah", "leah@mycompany.com", "Leah Johnson");
			await EnsureUser("ethan", "ethan@mycompany.com", "Ethan Castro");

			if (!await db.LeaveTypes.AnyAsync())
            {
                db.LeaveTypes.AddRange(
                    new LeaveType { Name = "Annual", MaxDurationDays = 30, IsActive = true },
                    new LeaveType { Name = "Sick", MaxDurationDays = 10, IsActive = true },
                    new LeaveType { Name = "Unpaid", MaxDurationDays = 365, IsActive = true }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}