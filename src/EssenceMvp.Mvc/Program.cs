using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<HealthStatus>("health_status");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<EssenceDbContext>(options => options.UseNpgsql(dataSource));

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAlphaService, AlphaService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IAlphaEvaluationService, AlphaEvaluationService>();
builder.Services.AddScoped<IHealthCalculationService, HealthCalculationService>();
builder.Services.AddScoped<IProjectDetailComposerService, ProjectDetailComposerService>();
builder.Services.AddScoped<ISnapshotService, SnapshotService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("ReactDev");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
