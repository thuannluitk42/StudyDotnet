using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
namespace BookApi.Extensions
{
	public static class DiagnosticsExtensions
	{
		public static IServiceCollection AddDiagnostics(this IServiceCollection services)
		{
			services.AddOpenTelemetry()
				.ConfigureResource(rb => rb.AddService("BookApi"))
				.WithTracing(tracingBuilder =>
				{
					tracingBuilder
					  .AddAspNetCoreInstrumentation()
					  .AddHttpClientInstrumentation()
					  .AddConsoleExporter();
				})
				.WithMetrics(metricsBuilder =>
				{
					metricsBuilder
					  .AddAspNetCoreInstrumentation()
					  .AddHttpClientInstrumentation()
					  .AddConsoleExporter();
				});

			return services;
		}
	}
}
