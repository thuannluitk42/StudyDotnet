using System.Reflection;
using System.Text;
using System.Text.Json;
using AspNetCoreRateLimit;
using BookApi.Authorization;
using BookApi.Binders;
using BookApi.Brokers;
using BookApi.Constraints;
using BookApi.Data;
using BookApi.HealthChecks;
using BookApi.Models;
using BookApi.Services;
using BookApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === 1. THE STANDARD DI ===
builder.Services.AddSingleton<IStorageBroker, InMemoryStorageBroker>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DepartmentHandler>();
builder.Services.AddTransient<BookApi.Services.ILogger, ConsoleLogger>();

// === 2. DATABASE + IDENTITY ===
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
					  ?? "Data Source=bookapi.db"));

builder.Services.AddIdentity<AppUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 6;
});

// === 3. FLUENTVALIDATION ===
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<BookForCreationDtoValidator>();

// === 4. JWT AUTHENTICATION ===
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
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
		};
	});

// === 5. POLICY-BASED AUTHORIZATION ===
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
	options.AddPolicy("MinimumAge", policy => policy.Requirements.Add(new MinimumAgeRequirement(18)));
	options.AddPolicy("RequireITDepartment", policy => policy.Requirements.Add(new DepartmentRequirement("IT")));
});

// === 6. MVC CONTROLLERS ===
builder.Services.AddControllers();

// === 7. SWAGGER ===
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

// === 8. RATE LIMITING ===
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// === 9. HEALTH CHECKS ===
builder.Services.AddHealthChecks()
	.AddSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
			   ?? "Data Source=bookapi.db", name: "SQLite")
	.AddCheck<MemoryHealthCheck>("Memory");

// === 10. HEALTH UI ===
builder.Services.AddHealthChecksUI(opt =>
{
	opt.SetEvaluationTimeInSeconds(10);
	opt.MaximumHistoryEntriesPerEndpoint(60);
	opt.SetApiMaxActiveRequests(1);
	opt.AddHealthCheckEndpoint("default api", "/health");
})
.AddSqliteStorage("Data Source=healthchecks_ui.db");

// === 11. CORS ===
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod());
});

// === 12. CUSTOM ROUTE CONSTRAINT ===
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

	// Auto migrate + Seed admin user
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
	await SeedData.InitializeAsync(scope.ServiceProvider);
}

// === MIDDLEWARE PIPELINE ===
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();

// === GLOBAL EXCEPTION HANDLER ===
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.StatusCode = 500;
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsync(JsonSerializer.Serialize(new
		{
			error = "Internal Server Error",
			message = "Something went wrong! Please try again later."
		}));
	});
});

// === CUSTOM 429 RESPONSE ===
app.Use(async (context, next) =>
{
	await next();
	if (context.Response.StatusCode == 429)
	{
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsync(JsonSerializer.Serialize(new
		{
			error = "Too Many Requests",
			retryAfter = context.Response.Headers["Retry-After"].ToString()
		}));
	}
});

// === HEALTH ENDPOINTS ===
app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(opt => opt.UIPath = "/health-ui");

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