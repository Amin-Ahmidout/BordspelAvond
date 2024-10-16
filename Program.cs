using BordspelAvond.Application.Interfaces;
using BordspelAvond.Application.Services;
using BordspelAvond.Data;
using BordspelAvond.Infrastructure;
using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection strings instellen
string bordspelDbConnectionString;
string authDbConnectionString;

if (builder.Environment.IsDevelopment())
{
    // Gebruik de connection strings uit appsettings.Development.json voor de ontwikkelomgeving
    bordspelDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    authDbConnectionString = builder.Configuration.GetConnectionString("AuthConnection")
        ?? throw new InvalidOperationException("Connection string 'AuthConnection' not found.");
}
else
{
    // Gebruik de environment variables in Azure voor de productieomgeving
    bordspelDbConnectionString = Environment.GetEnvironmentVariable("BORDSPELDB_CONNSTR")
        ?? throw new InvalidOperationException("Connection string 'BORDSPELDB_CONNSTR' not found.");

    authDbConnectionString = Environment.GetEnvironmentVariable("AUTHDB_CONNSTR")
        ?? throw new InvalidOperationException("Connection string 'AUTHDB_CONNSTR' not found.");
}

// Voeg BordspelDbContext toe
builder.Services.AddDbContext<BordspelDbContext>(options =>
    options.UseSqlServer(bordspelDbConnectionString));

// Voeg AuthDbContext toe
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(authDbConnectionString));

// Identity configureren met AuthDbContext
builder.Services.AddDefaultIdentity<Player>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddScoped<IGameNightService, GameNightService>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Zorg ervoor dat Authentication is ingeschakeld
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
