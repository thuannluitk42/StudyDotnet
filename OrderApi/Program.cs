using OrderApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
);

// Configure Services
builder.ConfigureDatabase();
builder.ConfigureRabbitMq();
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