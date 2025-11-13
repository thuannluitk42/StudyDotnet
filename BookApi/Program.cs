using System.Reflection;
using BookApi.Brokers;
using BookApi.Constraints;
using BookApi.Extensions;
using BookApi.Middleware;
using BookApi.Models;
using BookApi.Services;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// DI: THE STANDARD (Ch3.7)
builder.Services.AddSingleton<IStorageBroker, InMemoryStorageBroker>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddTransient<BookApi.Services.ILogger, ConsoleLogger>();

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Book API (.NET 10 RC2)",
		Version = "v1",
		Description = "Ch4: Pipeline + Custom Middleware"
	});

	// XML Comments: Ch12.7 p.297 → HOÀN HẢO
	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	c.IncludeXmlComments(xmlPath);
});

if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDiagnostics(); // ← MỚI TRONG .NET 10
}

// CORS: Chuẩn bị cho Blazor (Ch19) → TUYỆT VỜI
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.Configure<RouteOptions>(options =>
{
	options.ConstraintMap.Add("year", typeof(YearRouteConstraint));
});

var app = builder.Build();

// Development: Swagger UI
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRequestTiming();     // ← Custom middleware của bạn
app.UseCors();
app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.StatusCode = 500;
		await context.Response.WriteAsync("Something went wrong!");
	});
});

app.MapGet("/hello", () => "Hello from Minimal API!");
app.MapPost("/echo", (Book book) => Results.Ok(book))
   .WithName("EchoBook");

app.Run();