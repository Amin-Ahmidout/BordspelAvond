using BordspelAvond.Application.Interfaces;
using BordspelAvond.Application.Services;
using BordspelAvond.Data;
using BordspelAvond.Infrastructure;
using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Connection strings instellen
string bordspelDbConnectionString;
string authDbConnectionString;

if (builder.Environment.IsDevelopment())
{
    bordspelDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    authDbConnectionString = builder.Configuration.GetConnectionString("AuthConnection")
        ?? throw new InvalidOperationException("Connection string 'AuthConnection' not found.");
}
else
{
    bordspelDbConnectionString = builder.Configuration.GetConnectionString("BORDSPELDB_CONNSTR")
        ?? throw new InvalidOperationException("Connection string 'BORDSPELDB_CONNSTR' not found.");
    authDbConnectionString = builder.Configuration.GetConnectionString("AUTHDB_CONNSTR")
        ?? throw new InvalidOperationException("Connection string 'AUTHDB_CONNSTR' not found.");
}

// Voeg BordspelDbContext en AuthDbContext toe
builder.Services.AddDbContext<BordspelDbContext>(options =>
    options.UseSqlServer(bordspelDbConnectionString));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(authDbConnectionString));

// Configureer Identity met AuthDbContext
builder.Services.AddDefaultIdentity<Player>()
    .AddEntityFrameworkStores<AuthDbContext>();

// JWT authenticatie configureren
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});

// Dependency Injection configureren
builder.Services.AddScoped<IGameNightService, GameNightService>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

// Controllers met Views toevoegen
builder.Services.AddControllersWithViews();

// Voeg Swagger voor API-documentatie toe
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BordspelAvond API", Version = "v1" });
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Automatische migraties toepassen bij opstarten
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var bordspelContext = services.GetRequiredService<BordspelDbContext>();
        bordspelContext.Database.Migrate();

        var authContext = services.GetRequiredService<AuthDbContext>();
        authContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configureer het HTTP-verzoekpipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BordspelAvond API V1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Voeg authenticatie en autorisatie toe
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
