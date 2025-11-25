using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderApi.Consumers;
using OrderApi.Data;

namespace OrderApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            // Tạo folder data nếu chưa có
            var dataSource = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString).DataSource;
            var directory = Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            builder.Services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlite(connectionString);

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });
        }

        public static void ConfigureRabbitMq(this WebApplicationBuilder builder)
        {
            builder.Services.AddMassTransit(x =>
            {
                // Register consumers
                x.AddConsumer<BookCreatedEventConsumer>();

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

                    // Configure retry policy
                    cfg.UseMessageRetry(r => r.Exponential(5,
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(2)));

                    cfg.ConfigureEndpoints(context);
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
        }
    }
}