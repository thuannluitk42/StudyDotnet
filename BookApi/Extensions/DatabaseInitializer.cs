using BookApi.data;

namespace BookApi.Extensions
{
	public static class DatabaseInitializer
	{
		public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var provider = scope.ServiceProvider;

			var context = provider.GetRequiredService<AppDbContext>();
			var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

			logger.LogInformation("Ensuring database exists...");
			await context.Database.EnsureCreatedAsync();
			logger.LogInformation("Database ensured.");

			await SeedData.InitializeAsync(provider, context);
		}
	}
}
