using BordspelAvond.Application.Interfaces;
using BordspelAvond.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GraphQL;
using GraphQL.Server;
using GraphQL.Types;
using GraphQL.Server.Ui.Playground;
using BordspelAvond.Core.Domain;
using BordspelAvond.Core.Interfaces;
using BordspelAvond.Infrastructure.Repositories;
using BordspelAvond.Infrastructure.Data;
using BordspelAvond.Presentation.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// Connection strings instellen
string bordspelDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
string authDbConnectionString = builder.Configuration.GetConnectionString("AuthConnection")
    ?? throw new InvalidOperationException("Connection string 'AuthConnection' not found.");

// Voeg BordspelDbContext en AuthDbContext toe
builder.Services.AddDbContext<BordspelDbContext>(options => options.UseSqlServer(bordspelDbConnectionString));
builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(authDbConnectionString));

// Identity configureren
builder.Services.AddDefaultIdentity<Player>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddScoped<IGameNightService, GameNightService>();

// Configureer authentificatie: Cookies voor webapplicatie en JWT voor API
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key ontbreekt.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer ontbreekt.");



builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Dependency Injection configureren
builder.Services.AddScoped<IGameNightService, GameNightService>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

// GraphQL configuratie
builder.Services.AddScoped<ISchema, GameNightSchema>();
builder.Services.AddScoped<GameNightQuery>();
builder.Services.AddScoped<GameNightType>();
builder.Services.AddGraphQL(configure => configure.AddSystemTextJson());

// Controllers en Swagger configureren
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

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

// HTTP-verzoekpipeline configureren
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

// Authenticatie en autorisatie
app.UseAuthentication();
app.UseAuthorization();

// GraphQL middleware
app.UseGraphQL<ISchema>();
app.UseGraphQLPlayground("/graphql-playground");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
