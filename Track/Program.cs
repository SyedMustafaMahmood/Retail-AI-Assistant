using Microsoft.EntityFrameworkCore;
using Track.AI;
using Track.Data;
using Track.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient
builder.Services.AddHttpClient();

// AI + RAG
builder.Services.AddScoped<IAIClient, GeminiClient>();
builder.Services.AddScoped<IEmbeddingClient, EmbeddingClient>();

// Services
builder.Services.AddScoped<IPolicyQAService, PolicyQAService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<DataSeeder>();

// Recommendation services
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<EmbeddingBuilderService>();

var app = builder.Build();

// Seed DB
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();

        var embeddingBuilder = scope.ServiceProvider.GetRequiredService<EmbeddingBuilderService>();
        await embeddingBuilder.BuildAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seeding failed: " + ex.ToString());
    }
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();