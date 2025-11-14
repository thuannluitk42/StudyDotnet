// System + Microsoft
using System.Reflection;
using System.Text;
// Project namespaces
using BookApi.Binders;
using BookApi.Brokers;
using BookApi.Constraints;
using BookApi.Extensions;
using BookApi.Middleware;
using BookApi.Models;
using BookApi.Services;
using BookApi.Validators;
// FluentValidation
using FluentValidation;
using FluentValidation.AspNetCore;
// Microsoft Identity & OpenAPI
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === 1. THE STANDARD DI (Ch3.7) ===
builder.Services.AddSingleton<IStorageBroker, InMemoryStorageBroker>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddTransient<BookApi.Services.ILogger, ConsoleLogger>();

// === 2. FLUENTVALIDATION ===
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<BookForCreationDtoValidator>();

// === 3. JWT AUTHENTICATION & AUTHORIZATION ===
builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audienc"],
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
		};
	});
builder.Services.AddAuthorization();

// === 4. AUTH SERVICE DI ===
builder.Services.AddScoped<IAuthService, AuthService>();

// === 5. MVC + JSON ===
builder.Services.AddControllers()
	.AddNewtonsoftJson();

// === 6. SWAGGER + XML COMMENTS ===
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

// === 7. .NET 10 DI DIAGNOSTICS (DEV ONLY) ===
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDiagnostics();
}

// === 8. CORS ===
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod());
});

// === 9. CUSTOM ROUTE CONSTRAINT ===
builder.Services.Configure<RouteOptions>(options =>
{
	options.ConstraintMap["year"] = typeof(YearRouteConstraint);
});

var app = builder.Build();

// === DEVELOPMENT ONLY ===
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// === SECURITY & MIDDLEWARE ===
app.UseHttpsRedirection();     // 1. HTTPS
app.UseRequestTiming();        // 2. Timing
app.UseCors();                 // 3. CORS
app.UseAuthentication();       // 4. AuthN
app.UseAuthorization();        // 5. AuthZ

// === GLOBAL EXCEPTION HANDLER ===
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.StatusCode = 500;
		await context.Response.WriteAsync("Something went wrong! Please try again later.");
	});
});

// === ENDPOINTS ===
app.MapControllers();

// === MINIMAL API ===
app.MapGet("/hello", () => "Hello from Minimal API!");

app.MapPost("/echo", (Book book) => Results.Ok(book))
   .WithName("EchoBook");

app.MapGet("/api/books/by-date",
	([ModelBinder(BinderType = typeof(CustomDateBinder))] DateTime date) =>
		Results.Ok($"Selected date: {date:yyyy-MM-dd}"))
   .WithName("GetByDate");

app.Run();