using BookApi.Brokers;
using BookApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ADD SERVICES HERE (before Build!)
builder.Services.AddSingleton<IStorageBroker, InMemoryStorageBroker>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();