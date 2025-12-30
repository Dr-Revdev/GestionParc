using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DotNetEnv;
using GestiParc.Api.Services;
using GestiParc.Core.Interfaces.Repositories;
using GestiParc.Infrastructure;
using GestiParc.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Charge un fichier .env si présent (pas de secrets dans appsettings.json)
var envInBaseDir = Path.Combine(AppContext.BaseDirectory, ".env");
var envInCwd = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envInBaseDir)) Env.Load(envInBaseDir);
else if (File.Exists(envInCwd)) Env.Load(envInCwd);

var builder = WebApplication.CreateBuilder(args);

// DB
var cs = builder.Configuration.GetConnectionString("GestiParcDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("ConnectionStrings__GestiParcDb manquante (définir via env / .env). ");
DbFactory.ConnectionString = cs;

// Controllers
builder.Services.AddControllers();

// JWT (LAN, simple)
const string jwtIssuer = "GestiParc";
const string jwtAudience = "GestiParc.Ui";
const int jwtExpirationMinutes = 8 * 60;

var jwtSecret = builder.Configuration["Jwt:Secret"]
                ?? builder.Configuration["JWT_SECRET"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Jwt__Secret manquant (définir via env / .env). ");
if (jwtSecret.Trim().Length < 32)
    throw new InvalidOperationException("Jwt__Secret doit faire au moins 32 caractères.");

builder.Services.Configure<JwtOptions>(o =>
{
    o.Issuer = jwtIssuer;
    o.Audience = jwtAudience;
    o.Secret = jwtSecret;
    o.ExpirationMinutes = jwtExpirationMinutes;
});
builder.Services.AddSingleton<JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // IMPORTANT: on garde les types de claims tels quels ("sub", "role", etc.)
        // Sinon, le mapping peut transformer "role" en ClaimTypes.Role et casser les policies.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("USER", "ADMIN"));
});

// DI repositories
builder.Services.AddScoped<IEquipmentRepository, EquipmentMySqlRepository>();
builder.Services.AddScoped<IEquipmentTypeRepository, EquipmentTypeMySqlRepository>();
builder.Services.AddScoped<IAgentRepository, AgentMySqlRepository>();
builder.Services.AddScoped<ISiteRepository, SiteMySqlRepository>();
builder.Services.AddScoped<IEquipeRepository, EquipeMySqlRepository>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurMySqlRepository>();

// Swagger (LAN : OK de le laisser en dev)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "GestiParc.Api", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Entrez un token JWT au format: Bearer {token}"
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
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();