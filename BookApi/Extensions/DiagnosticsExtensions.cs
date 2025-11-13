using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
namespace BookApi.Extensions
{
	public static class DiagnosticsExtensions
	{
		/// <summary>
		/// Adds OpenTelemetry tracing & metrics for the API.
		/// Requires: OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Exporter.Console
		/// </summary>
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