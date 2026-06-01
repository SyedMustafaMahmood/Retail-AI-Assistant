using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Track.AI;
using Track.Data;
using Track.Repositories.Implementations;
using Track.Repositories.Interfaces;
using Track.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================
// DATABASE
// =========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

// =========================
// JWT AUTHENTICATION
// =========================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )),
                RoleClaimType = ClaimTypes.Role
            };
    });

// =========================
// AUTHORIZATION
// =========================
builder.Services.AddAuthorization();

// =========================
// CORS
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// =========================
// CONTROLLERS
// =========================
builder.Services.AddControllers();

// =========================
// SWAGGER
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// HTTP CLIENT
// =========================
builder.Services.AddHttpClient();

// =========================
// AI SERVICES
// =========================
builder.Services.AddScoped<IAIClient, GeminiClient>();
builder.Services.AddScoped<IEmbeddingClient, EmbeddingClient>();

// =========================
// REPOSITORIES
// =========================
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IQueryLogRepository, QueryLogRepository>();
builder.Services.AddScoped<IRequestLogRepository, RequestLogRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IEmbeddingRepository, EmbeddingRepository>();
builder.Services.AddScoped<IRecommendationLogRepository, RecommendationLogRepository>();

// =========================
// APPLICATION SERVICES
// =========================
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IPolicyQAService, PolicyQAService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<EmbeddingBuilderService>();

// =========================
// API VERSIONING
// =========================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc();

var app = builder.Build();

// =========================
// APPLY MIGRATIONS + SEED DATA
// =========================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        db.Database.Migrate();
        await DataSeeder.SeedAsync(db);

        var embeddingBuilder = scope.ServiceProvider
            .GetRequiredService<EmbeddingBuilderService>();

        await embeddingBuilder.BuildAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seeding failed:");
        Console.WriteLine(ex.ToString());
    }
}

// =========================
// ADMIN SEEDER
// =========================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        db.Database.Migrate();
        await DataSeeder.SeedAsync(db);
        AdminSeeder.SeedAdmin(db);

        var embeddingBuilder = scope.ServiceProvider
            .GetRequiredService<EmbeddingBuilderService>();

        await embeddingBuilder.BuildAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

// =========================
// MIDDLEWARE
// =========================
app.UseCors("AllowAngular");        
app.UseHttpsRedirection();

// =========================
// SWAGGER UI
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =========================
// AUTHENTICATION + AUTHORIZATION
// =========================
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();             

// =========================
// MAP CONTROLLERS
// =========================
app.MapControllers();

// =========================
// RUN APPLICATION
// =========================
app.Run();