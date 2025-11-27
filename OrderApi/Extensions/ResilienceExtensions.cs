using Polly;
using Polly.Extensions.Http;

namespace OrderApi.Extensions;

public static class ResilienceExtensions
{
    public static IServiceCollection AddResiliencePatterns(this IServiceCollection services)
    {
        // Configure HttpClient with Retry and Circuit Breaker policies
		services.AddHttpClient("BookApiClient", client =>
		{
			client.BaseAddress = new Uri("http://bookapi:80");
			client.Timeout = TimeSpan.FromSeconds(30);
		})
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    /// <summary>
    /// Retry policy: Retry 3 times with exponential backoff
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handle 5xx and 408 errors
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"🔄 Retry {retryCount} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });
    }

    /// <summary>
    /// Circuit Breaker policy: Open circuit after 5 consecutive failures, wait 30 seconds before retry
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Console.WriteLine($"⚠️ Circuit breaker opened for {duration.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit breaker reset - requests will flow again");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("🔍 Circuit breaker half-open - testing if service recovered");
                });
    }
}