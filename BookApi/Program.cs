using BookApi.Extensions;
using Serilog;
using BookApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
	lc.ReadFrom.Configuration(ctx.Configuration)
);

// Configure Services
builder.ConfigureDatabase();
builder.ConfigureIdentity();
builder.ConfigureJwt();
builder.ConfigureCors();
builder.ConfigureControllers();
builder.ConfigureCustomServices();
builder.ConfigureRabbitMq();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();


// Register route constraints
builder.Services.Configure<RouteOptions>(options =>
{
	options.ConstraintMap.Add("year", typeof(BookApi.Constraints.YearRouteConstraint));
});

// builder.WebHost.ConfigureKestrel(options =>
// {
//     // HTTP endpoint for REST API
//     options.ListenLocalhost(5238, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
// });
// Only configure Kestrel for local development
// Configure Kestrel for HTTP/2 support
builder.WebHost.ConfigureKestrel(options =>
{
    // In Docker, listen on all interfaces with HTTP/2
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        options.ConfigureEndpointDefaults(listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        });
    }
    // In local development, listen on localhost:5238 with HTTP/2
    else
    {
        options.ListenLocalhost(5238, o =>
        {
            o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        });
    }
});

var app = builder.Build();

// Apply migrations and seed data
await app.ApplyMigrationsAndSeedAsync();

// Middleware pipeline
app.UseCustomMiddleware();

// Map endpoints
app.MapControllers();
app.MapGrpcService<BookGrpcService>();
app.MapGrpcReflectionService();
app.MapHealthChecks("/health");
app.MapHealthChecksUI(opt => opt.UIPath = "/health-ui");

await app.RunAsync();
