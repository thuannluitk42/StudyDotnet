using Serilog.Context;

namespace BookApi.Middleware
{
	public class UserInfoEnricherMiddleware
	{
		private readonly RequestDelegate _next;

		public UserInfoEnricherMiddleware(RequestDelegate next) => _next = next;

		public async Task InvokeAsync(HttpContext context)
		{
			var userId = context.User.Identity?.IsAuthenticated == true
				? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous"
				: "Anonymous";

			// L?y CorrelationId t? context
			var correlationId = context.TraceIdentifier;

			using (LogContext.PushProperty("UserId", userId))
			using (LogContext.PushProperty("UserEmail", context.User.Identity?.Name ?? "Anonymous"))
			using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"))
			using (LogContext.PushProperty("CorrelationId", correlationId))
			{
				await _next(context);
			}
		}
	}
}
