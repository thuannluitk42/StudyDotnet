using Grpc.Core;
using Polly;
using Polly.Extensions.Http;

namespace OrderApi.Extensions;

public static class GrpcResilienceExtensions
{
    public static IServiceCollection AddGrpcClientWithResilience(this IServiceCollection services, IConfiguration configuration)
    {
        var grpcAddress = configuration["BookApi:GrpcAddress"] ?? "http://localhost:5238";

        services.AddGrpcClient<BookApi.Protos.BookService.BookServiceClient>(options =>
        {
            options.Address = new Uri(grpcAddress);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"🔄 Retry {retryCount} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Console.WriteLine($"⚡ Circuit breaker opened for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit breaker reset");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
    }
}