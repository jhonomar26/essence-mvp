using System.Text;
using EssenceMvp.Api.Features;
using EssenceMvp.Api.Infrastructure;
using EssenceMvp.Api.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<HealthStatus>("health_status");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<EssenceDbContext>(options =>
    options.UseNpgsql(dataSource));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) ||
    jwtKey == "dev-only-change-me-please-dev-only-change-me")
{
    throw new InvalidOperationException(
        "JWT signing key is not configured or is using the insecure development default. Configure Jwt:Key before starting the application.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EssenceMvp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EssenceMvp";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EssenceDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapAlphaEndpoints();

app.MapGet("/health", async (EssenceDbContext db) =>
{
    var alphaCount = await db.Alphas.CountAsync();
    return Results.Ok(new { status = "ok", alphas = alphaCount });
});

app.Run();
