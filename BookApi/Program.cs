// Program.cs
using System.Reflection;
using System.Text;
using BookApi.Binders;
using BookApi.Brokers;
using BookApi.Constraints;
using BookApi.Extensions;
using BookApi.Middleware;
using BookApi.Models;
using BookApi.Services;
using BookApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === DI: THE STANDARD (Ch3.7) ===
builder.Services.AddSingleton<IStorageBroker, InMemoryStorageBroker>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddTransient<BookApi.Services.ILogger, ConsoleLogger>();

// === FluentValidation: TÁCH RIÊNG ===
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<BookForCreationDtoValidator>();

// === JWT Authentication ===
builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options =>
	{
		options.TokenValidationParameters = new()
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
		};
	});

builder.Services.AddAuthorization();
// === DI ===
builder.Services.AddScoped<IAuthService, AuthService>();

// === MVC + NewtonsoftJson ===
builder.Services.AddControllers()
	.AddNewtonsoftJson();

// === Swagger + XML Comments ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Book API (.NET 10 RC2)",
		Version = "v1",
		Description = "Mastering ASP.NET Core 10 — Mabrouk Mahdhi"
	});

	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	if (File.Exists(xmlPath))
		c.IncludeXmlComments(xmlPath);
});

// === .NET 10 DI Diagnostics ===
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDiagnostics();
}

// === CORS ===
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod());
});

// === Custom Route Constraint ===
builder.Services.Configure<RouteOptions>(options =>
{
	options.ConstraintMap["year"] = typeof(YearRouteConstraint);
});

var app = builder.Build();

// === Pipeline ===
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRequestTiming();
app.UseCors();

app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.StatusCode = 500;
		await context.Response.WriteAsync("Something went wrong! Please try again later.");
	});
});

app.MapControllers();

// === Minimal API ===
app.MapGet("/hello", () => "Hello from Minimal API!");

app.MapPost("/echo", (Book book) => Results.Ok(book))
   .WithName("EchoBook");

app.MapGet("/api/books/by-date",
	([ModelBinder(BinderType = typeof(CustomDateBinder))] DateTime date) =>
		Results.Ok($"Selected date: {date:yyyy-MM-dd}"))
   .WithName("GetByDate");

app.Run();