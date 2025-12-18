using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AspNetCoreRateLimit;
using GestiParc.Api.Services;
using GestiParc.Core.Interfaces.Repositories;
using GestiParc.Infrastructure;
using GestiParc.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurer la connection string pour DbFactory
var cs = builder.Configuration.GetConnectionString("GestiParcDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Connection string 'GestiParcDb' manquante dans appsettings.json.");

DbFactory.ConnectionString = cs;

// 2. Controllers
builder.Services.AddControllers();

// 2a. Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// 2b. JWT options
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];
var jwtSecret = jwtSection["Secret"];
var jwtExpirationMinutes = jwtSection.GetValue<int?>("ExpirationMinutes") ?? 60;

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Configuration JWT manquante : Jwt:Issuer");
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Configuration JWT manquante : Jwt:Audience");
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Configuration JWT manquante : Jwt:Secret (définir via variable d'environnement Jwt__Secret)");
if (jwtExpirationMinutes <= 0)
    throw new InvalidOperationException("Configuration JWT invalide : Jwt:ExpirationMinutes doit être > 0");

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddSingleton<JwtTokenService>();

// 2c. AuthN/AuthZ (API en HTTP derrière un reverse proxy HTTPS)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
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
    // Politique par défaut : utilisateur authentifié
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
    
    // Politique Admin : rôle ADMIN requis
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("ADMIN"));
    
    // Politique UserOrAdmin : rôle USER ou ADMIN
    options.AddPolicy("UserOrAdmin", policy => 
        policy.RequireRole("USER", "ADMIN"));
});

// 3. DI : repository équipements
builder.Services.AddScoped<IEquipmentRepository, EquipmentMySqlRepository>();
builder.Services.AddScoped<IEquipmentTypeRepository, EquipmentTypeMySqlRepository>();
builder.Services.AddScoped<IAgentRepository, AgentMySqlRepository>();
builder.Services.AddScoped<ISiteRepository, SiteMySqlRepository>();
builder.Services.AddScoped<IEquipeRepository, EquipeMySqlRepository>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurMySqlRepository>();

// Swagger seulement en Development + support Bearer
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
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();