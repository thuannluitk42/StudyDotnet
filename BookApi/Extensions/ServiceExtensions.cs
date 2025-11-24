using AspNetCoreRateLimit;
using BookApi.Authorization;
using BookApi.Brokers;
using BookApi.Data;
using BookApi.Models;
using BookApi.Services;
using BookApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Extensions;

public static class ServiceExtensions
{
	public static void ConfigureDatabase(this WebApplicationBuilder builder)
	{
		// Lấy nguyên connection string
		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

		// Tạo folder nếu chưa có
		var dataSource = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString).DataSource;
		Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);

		builder.Services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlite(connectionString, sqliteOptions =>
			{
				sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				sqliteOptions.CommandTimeout(30);
			});

			if (builder.Environment.IsDevelopment())
			{
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
			}
		});
	}

	public static void ConfigureIdentity(this WebApplicationBuilder builder)
	{
		builder.Services.AddIdentity<AppUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
		{
			options.Password.RequireDigit = true;
			options.Password.RequireLowercase = true;
			options.Password.RequireUppercase = true;
			options.Password.RequireNonAlphanumeric = true;
			options.Password.RequiredLength = 8;
			options.Password.RequiredUniqueChars = 4;

			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
			options.Lockout.MaxFailedAccessAttempts = 5;

			options.User.RequireUniqueEmail = true;
		})
		.AddEntityFrameworkStores<AppDbContext>()
		.AddDefaultTokenProviders();
	}

	public static void ConfigureJwt(this WebApplicationBuilder builder)
	{
		var jwtKey = builder.Configuration["Jwt:Key"]!;
		var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
		var jwtAudience = builder.Configuration["Jwt:Audience"]!;

		builder.Services.AddAuthentication("Bearer")
			.AddJwtBearer("Bearer", options =>
			{
				options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtIssuer,
					ValidAudience = jwtAudience,
					IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
					ClockSkew = TimeSpan.FromMinutes(5)
				};
			});
	}

	public static void ConfigureCors(this WebApplicationBuilder builder)
	{
		builder.Services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
			});
		});
	}

	public static void ConfigureControllers(this WebApplicationBuilder builder)
	{
		builder.Services.AddControllers()
			.AddJsonOptions(opt =>
			{
				opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
				opt.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
			});

		builder.Services.AddFluentValidationAutoValidation();
		builder.Services.AddFluentValidationClientsideAdapters();
		builder.Services.AddValidatorsFromAssemblyContaining<BookForCreationDtoValidator>();
	}

	public static void ConfigureCustomServices(this WebApplicationBuilder builder)
	{
		// Book services
		// Chuyển nguồn dữ liệu từ inmemory sang lấy dữ liệu từ bookapi.db
		//builder.Services.AddSingleton<IStorageBroker, InMemoryStorageBroker>();
		builder.Services.AddScoped<IStorageBroker, DatabaseStorageBroker>();
		builder.Services.AddDistributedMemoryCache();
		builder.Services.AddScoped<IBookService, BookService>();
		builder.Services.AddScoped<IAuthService, AuthService>();

		// Authorization Handlers
		builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, MinimumAgeHandler>();
		builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, DepartmentHandler>();

		// Logger
		builder.Services.AddTransient<BookApi.Services.ILogger, ConsoleLogger>();

		// HealthChecks
		builder.Services.AddHealthChecks();
		builder.Services.AddHealthChecksUI(setupSettings: setup =>
		{
			setup.SetEvaluationTimeInSeconds(10);
			setup.MaximumHistoryEntriesPerEndpoint(60);
		})
		.AddSqliteStorage("Data Source=healthchecks.db");

		// Rate Limiting
		builder.Services.AddMemoryCache();
		builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
		builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimiting"));
		builder.Services.AddInMemoryRateLimiting();
		builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
	}

	public static void ConfigureRabbitMq(this WebApplicationBuilder builder)
	{
		builder.Services.AddMassTransit(x =>
		{
			// Register consumers
			x.AddConsumer<BookApi.Consumers.EmailConsumer>();
			x.AddConsumer<BookApi.Consumers.BookAnalyticsConsumer>();

			x.UsingRabbitMq((context, cfg) =>
			{
				var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
				var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
				var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

				cfg.Host(rabbitHost, "/", h =>
				{
					h.Username(rabbitUser);
					h.Password(rabbitPass);
				});

				// Configure retry policy: 5 retries with exponential backoff
				cfg.UseMessageRetry(r => r.Exponential(5,
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(30),
					TimeSpan.FromSeconds(2)));

				cfg.ConfigureEndpoints(context);
			});
		});
	}
}
