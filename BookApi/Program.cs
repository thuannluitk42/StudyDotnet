using System.Reflection;
using BookApi.Brokers;
using BookApi.Middleware;
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

	// .NET 10 RC2: Comment out OpenAPI 3.1 → ĐÚNG
	// c.SupportOpenApi3_1();

	// XML Comments: Ch12.7 p.297 → HOÀN HẢO
	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	c.IncludeXmlComments(xmlPath);
});

// CORS: Chuẩn bị cho Blazor (Ch19) → TUYỆT VỜI
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Development: Swagger UI
if (app.Environment.IsDevelopment())
{

	app.UseSwagger();
	app.UseSwaggerUI();
}

// PIPELINE: CHÍNH XÁC THỨ TỰ (Ch4.2 p.70)
app.UseHttpsRedirection();
app.UseRequestTiming();     // ← Custom middleware của bạn
app.UseCors();              // ← ĐÃ THÊM — HOÀN HẢO
app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.StatusCode = 500;
		await context.Response.WriteAsync("Something went wrong!");
	});
});

app.Run();