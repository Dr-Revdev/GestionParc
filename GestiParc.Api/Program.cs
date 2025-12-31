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
using Serilog;
using Serilog.Events;
using Serilog.Context;
using GestiParc.Api.Logging;

var builder = WebApplication.CreateBuilder(args);

// Logs (Docker-friendly): console + fichier journalier (rétention 30 jours)
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var logDir = context.Configuration["LOG_DIR"] ?? "logs";
    try { Directory.CreateDirectory(logDir); } catch { /* ignore */ }

    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "GestiParc.Api")
        .ReadFrom.Services(services)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] rid={RequestId} uid={UserId} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: Path.Combine(logDir, "gestiparc-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] rid={RequestId} uid={UserId} {Message:lj}{NewLine}{Exception}");
});

static bool IsDevelopmentEnvironment()
{
    var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    var env = string.IsNullOrWhiteSpace(aspnetEnv) ? dotnetEnv : aspnetEnv;
    return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
}

static string? FindDotEnvPath(string? explicitPath)
{
    if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath))
        return explicitPath;

    static string? FindInParents(string startDir)
    {
        try
        {
            var dir = new DirectoryInfo(startDir);
            for (var i = 0; i < 8 && dir is not null; i++)
            {
                var candidate = Path.Combine(dir.FullName, ".env");
                if (File.Exists(candidate))
                    return candidate;

                dir = dir.Parent;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    // Priorité: cwd (ex: dotnet run depuis le projet), puis base dir (bin), puis recherche dans les parents.
    var envInCwd = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envInCwd))
        return envInCwd;

    var envInBaseDir = Path.Combine(AppContext.BaseDirectory, ".env");
    if (File.Exists(envInBaseDir))
        return envInBaseDir;

    return FindInParents(Directory.GetCurrentDirectory())
           ?? FindInParents(AppContext.BaseDirectory);
}

// Charge un fichier .env uniquement en développement (évite de dépendre de .env en prod / docker)
if (IsDevelopmentEnvironment())
{
    var dotenvPath = FindDotEnvPath(Environment.GetEnvironmentVariable("GESTIPARC_DOTENV_PATH"));
    if (!string.IsNullOrWhiteSpace(dotenvPath))
        Env.Load(dotenvPath);
}

// Docker secrets: montage typique dans /run/secrets (les noms de fichiers utilisent "__" comme séparateur)
// Exemple: Jwt__Secret, ConnectionStrings__GestiParcDb
var dockerSecretsDir = builder.Configuration["DOCKER_SECRETS_DIR"] ?? "/run/secrets";
if (Directory.Exists(dockerSecretsDir))
{
    builder.Configuration.AddKeyPerFile(dockerSecretsDir, optional: true);
}

// DB
var cs = builder.Configuration.GetConnectionString("GestiParcDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("ConnectionStrings__GestiParcDb manquante (définir via env / .env / Docker secret). ");
DbFactory.ConnectionString = cs;

// Controllers
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<AuditActionFilter>();
});

// JWT (LAN, simple)
const string jwtIssuer = "GestiParc";
const string jwtAudience = "GestiParc.Ui";
const int jwtExpirationMinutes = 8 * 60;

var jwtSecret = builder.Configuration["Jwt:Secret"]
                ?? builder.Configuration["JWT_SECRET"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Jwt__Secret manquant (définir via env / .env / Docker secret). ");
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

// Corrélation / audit: enrichit tous les logs avec RequestId + UserId (sub) sans exposer de pseudo.
app.Use(async (context, next) =>
{
    var requestId = context.TraceIdentifier;
    var endpoint = $"{context.Request.Method} {context.Request.Path}";
    var userId = context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                 ?? context.User?.FindFirst("sub")?.Value;

    using (LogContext.PushProperty("RequestId", requestId))
    using (LogContext.PushProperty("Endpoint", endpoint))
    using (LogContext.PushProperty("UserId", string.IsNullOrWhiteSpace(userId) ? null : userId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} => {StatusCode} ({Elapsed:0.0000} ms)";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
        diagnosticContext.Set("Endpoint", $"{httpContext.Request.Method} {httpContext.Request.Path}");

        var userId = httpContext.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? httpContext.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
            diagnosticContext.Set("UserId", userId);
    };
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }