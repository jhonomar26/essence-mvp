using EssenceMvp.Api.Infrastructure;
using EssenceMvp.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", async (EssenceDbContext db) =>
{
    var alphaCount = await db.Alphas.CountAsync();
    return Results.Ok(new { status = "ok", alphas = alphaCount });
});

app.Run();
