using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Track.AI;
using Track.Data;
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

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

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
// APPLICATION SERVICES
// =========================
builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<IPolicyQAService, PolicyQAService>();

builder.Services.AddScoped<TicketService>();

builder.Services.AddScoped<RecommendationService>();

builder.Services.AddScoped<EmbeddingBuilderService>();


//Api versioining
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false;
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

        // Create DB + Apply Migrations
        db.Database.Migrate();

        // Seed Initial Data
        await DataSeeder.SeedAsync(db);

        // Build Product Embeddings
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
//Admin Seeder
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // Apply migrations
        db.Database.Migrate();

        // Seed products + transactions
        await DataSeeder.SeedAsync(db);

        // Seed admin
        AdminSeeder.SeedAdmin(db);

        // Build embeddings
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



// =========================
// MAP CONTROLLERS
// =========================
app.MapControllers();



// =========================
// RUN APPLICATION
// =========================
app.Run();