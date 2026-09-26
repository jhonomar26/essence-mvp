using EssenceMvp.Application;
using EssenceMvp.Infrastructure;
using EssenceMvp.Mvc.Auth;
using EssenceMvp.Mvc.Filters;
using EssenceMvp.Mvc.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationActionFilter>();
});
builder.Services.AddValidatorsFromAssemblyContaining<EvaluateChecklistViewModelValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddAuthentication("SessionToken")
    .AddScheme<AuthenticationSchemeOptions, SessionTokenAuthenticationHandler>("SessionToken", null);

builder.Services.AddAuthorization();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
