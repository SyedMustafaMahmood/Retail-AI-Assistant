using Microsoft.EntityFrameworkCore;
using Track.AI;
using Track.Data;
using Track.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Gemini AI (FREE)
builder.Services.AddHttpClient<IAIClient, GeminiClient>();

// Business service
builder.Services.AddScoped<TicketService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();