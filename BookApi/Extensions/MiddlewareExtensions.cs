using AspNetCoreRateLimit;
using BookApi.Middleware;
using Serilog;

namespace BookApi.Extensions
{
	public static class MiddlewareExtensions
	{
		public static void UseCustomMiddleware(this WebApplication app)
		{
			app.UseExceptionHandler("/error");
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseSerilogRequestLogging();
			app.UseRouting();

			app.UseIpRateLimiting();

			app.UseCors();
			app.UseAuthentication();
			app.UseMiddleware<UserInfoEnricherMiddleware>();
			app.UseAuthorization();
		}
	}
}
