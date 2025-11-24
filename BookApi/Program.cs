using BookApi.Extensions;
using Serilog;

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

// Register route constraints
builder.Services.Configure<RouteOptions>(options =>
{
	options.ConstraintMap.Add("year", typeof(BookApi.Constraints.YearRouteConstraint));
});

var app = builder.Build();

// Apply migrations and seed data
await app.ApplyMigrationsAndSeedAsync();

// Middleware pipeline
app.UseCustomMiddleware();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecksUI(opt => opt.UIPath = "/health-ui");

await app.RunAsync();
