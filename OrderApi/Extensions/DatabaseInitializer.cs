using Microsoft.EntityFrameworkCore;
using OrderApi.Data;

namespace OrderApi.Extensions
{
    public static class DatabaseInitializer
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            
            try
            {
                await context.Database.MigrateAsync();
                app.Logger.LogInformation("✅ Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "❌ Error applying database migrations");
                throw;
            }
        }
    }
}