using BookApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Data;

public static class SeedData
{
	public static async Task InitializeAsync(IServiceProvider services, AppDbContext context)
	{
		var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
		var userMgr = services.GetRequiredService<UserManager<AppUser>>();
		var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();

		if (userMgr == null || roleMgr == null)
		{
			logger.LogWarning("Missing services for seeding.") ;
			return;
		}

		// Check if already seeded
		bool alreadySeeded = false;
		try
		{
			alreadySeeded = await userMgr.Users.AnyAsync() || await context.Books.AnyAsync();
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Error checking if database is seeded.Assuming not seeded.");
			alreadySeeded = false;
		}

		if (alreadySeeded)
		{
			logger.LogInformation("Database already seeded.Skipping initialization.");
			return;
		}

		// --- Seed Roles ---
		var roles = new[] { "Admin", "User" };
		foreach (var roleName in roles)
		{
			if (!await roleMgr.RoleExistsAsync(roleName))
			{
				var role = new IdentityRole(roleName);
				var res = await roleMgr.CreateAsync(role);
				if (res.Succeeded)
					logger.LogInformation("Created role { Role}", roleName);

				else
					logger.LogWarning("Failed to create role { Role}: { Errors}", roleName, string.Join(',', res.Errors.Select(e => e.Description)));
			}
		}

		// --- Seed Admin User ---
		var adminEmail = "studydotnet@yopmail.com";
		var adminPassword = "Abc12345@";

		var admin = new AppUser
		{
			UserName = "admin",
			Email = adminEmail,
			EmailConfirmed = true
		};

		var createRes = await userMgr.CreateAsync(admin, adminPassword);
		if (createRes.Succeeded)
		{
			logger.LogInformation("Created admin user { Email}", adminEmail);
			await userMgr.AddToRoleAsync(admin, "Admin");
		}
		else
		{
			logger.LogWarning("Failed to create admin user: { Errors}", string.Join(',', createRes.Errors.Select(e => e.Description)));
		}

		var books = new[]
		{
			new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", PublishedYear = 1999 },
			new Book { Title = "Clean Code", Author = "Robert C. Martin", PublishedYear = 2008 }
		};
		context.Books.AddRange(books);
		await context.SaveChangesAsync();
		logger.LogInformation("Seeded sample books.");

		logger.LogInformation("Database initialization and seeding completed!");
	}
}