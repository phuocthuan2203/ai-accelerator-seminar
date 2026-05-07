using System;
using System.IO;
using ToolLendingPlatform.Infrastructure.Data;
using ToolLendingPlatform.Infrastructure.Repositories;
using ToolLendingPlatform.Application.Services;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Api.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// ===== Configuration =====
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

// ===== Add Services =====

// Database
builder.Services.AddDbContext<ToolLendingDbContext>(options =>
    options.UseSqlite(connectionString));

// Session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Controllers
builder.Services.AddControllers();

// CORS (for local dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", corsBuilder =>
    {
        corsBuilder
            .WithOrigins("http://localhost:8000", "http://127.0.0.1:8000", "http://localhost:8123", "http://127.0.0.1:8123")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Distributed cache (required for sessions)
builder.Services.AddDistributedMemoryCache();

// ===== Rule #11: Dependency Injection for Authentication =====

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Application Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<AuthenticationService>();

// Filters
builder.Services.AddScoped<AuthExceptionFilter>(); // Rule #12

// Health checks (optional, for monitoring)
builder.Services.AddHealthChecks();

// ===== Build App =====
var app = builder.Build();

// ===== Database Migration =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToolLendingDbContext>();
    db.Database.Migrate();
}

// ===== Configure Middleware Pipeline =====

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Static files (frontend)
app.UseStaticFiles();

// CORS
app.UseCors("AllowLocalhost");

// Session middleware MUST come before routing
app.UseSession();

// Routing
app.UseRouting();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// Fallback: serve index.html for SPA (optional, for TASK-06+)
app.MapFallbackToFile("index.html");

app.Run("http://localhost:5123");
