using OrderApi.Extensions;
using Serilog;
using OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
);

// Configure Services
builder.ConfigureDatabase();
builder.ConfigureRabbitMq();
builder.Services.AddResiliencePatterns();
// Add gRPC client with Polly resilience patterns
builder.Services.AddGrpcClientWithResilience(builder.Configuration);
// Keep BookGrpcClient as wrapper (optional - for backward compatibility)
builder.Services.AddSingleton<BookGrpcClient>();
builder.ConfigureControllers();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations
await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Logger.LogInformation("🚀 OrderApi is running on {Urls}", 
    string.Join(", ", app.Urls));

await app.RunAsync();
public partial class Program { }