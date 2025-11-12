using System.Diagnostics;

namespace BookApi.Middleware;

public class RequestTimingMiddleware
{
	private readonly RequestDelegate _next;

	public RequestTimingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var stopwatch = Stopwatch.StartNew();
		await _next(context);  // Chạy middleware tiếp theo
		stopwatch.Stop();

		var time = stopwatch.ElapsedMilliseconds;
		Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path} - {time}ms");
	}
}
