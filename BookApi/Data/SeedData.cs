using BookApi.Models;
using Microsoft.AspNetCore.Identity;

namespace BookApi.Data;

public static class SeedData
{
	public static async Task InitializeAsync(IServiceProvider serviceProvider)
	{
		var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
		var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		// Tạo role
		string[] roles = { "Admin", "User" };
		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
				await roleManager.CreateAsync(new IdentityRole(role));
		}

		// Tạo user admin
		var adminEmail = "studydotnet@yopmail.com";
		var admin = await userManager.FindByEmailAsync(adminEmail);
		if (admin == null)
		{
			admin = new AppUser
			{
				UserName = adminEmail,
				Email = adminEmail,
				FullName = "Thuan Le Van",
				EmailConfirmed = true
			};

			var result = await userManager.CreateAsync(admin, "Abc12345@");
			if (result.Succeeded)
				await userManager.AddToRoleAsync(admin, "Admin");
		}
	}
}