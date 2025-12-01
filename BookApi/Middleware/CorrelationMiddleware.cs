using System.Diagnostics;
using Serilog.Context;

namespace BookApi.Middleware
{
	public class CorrelationMiddleware
	{
		private readonly RequestDelegate _next;
		private const string CorrelationHeaderName = "X-Correlation-ID";

		public CorrelationMiddleware(RequestDelegate next) => _next = next;

		public async Task InvokeAsync(HttpContext context)
		{
			string? correlationId = context.Request.Headers[CorrelationHeaderName];

			if (string.IsNullOrWhiteSpace(correlationId))
			{
				correlationId = Activity.Current?.TraceId.ToString();
			}

			if (string.IsNullOrWhiteSpace(correlationId))
			{
				correlationId = Guid.NewGuid().ToString();
			}

			try
			{
				context.TraceIdentifier = correlationId;
			}
			catch
			{

			}

			if (!context.Response.HasStarted)
			{
				context.Response.OnStarting(() =>
				{
					if (!context.Response.Headers.ContainsKey(CorrelationHeaderName))
						context.Response.Headers.Add(CorrelationHeaderName, correlationId);

					return Task.CompletedTask;
				});
			}

			using (LogContext.PushProperty("CorrelationId", correlationId))
			{
				await _next(context);
			}
		}
	}

	public static class CorrelationMiddlewareExtensions
	{
		public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder app) =>
			app.UseMiddleware<CorrelationMiddleware>();
	}
}
