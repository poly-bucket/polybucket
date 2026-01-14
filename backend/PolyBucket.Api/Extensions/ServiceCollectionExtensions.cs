using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PolyBucket.Api.Data;
using PolyBucket.Api.Settings;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.Plugins;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.SystemSettings.Plugins;
using PolyBucket.Api.Features.ACL.Services;
using System.Security.Claims;
using System.Linq;

namespace PolyBucket.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPolyBucketServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        return services;
    }

    public static IServiceCollection AddPolyBucketDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PolyBucketDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddPolyBucketAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        if (appSettings?.Security == null)
        {
            throw new InvalidOperationException("AppSettings:Security configuration is missing");
        }

        var jwtSecret = configuration["AppSettings:Security:JwtSecret"] 
            ?? configuration["JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? appSettings.Security.JwtSecret;

        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new InvalidOperationException(
                "JWT Secret must be configured via AppSettings:Security:JwtSecret, JWT_SECRET environment variable, or appsettings.json");
        }

        if (jwtSecret.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT Secret must be at least 32 characters long for security");
        }

        if (jwtSecret == "your-super-secret-key-with-at-least-32-characters-12345678")
        {
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            if (!environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Default JWT Secret cannot be used in production. Please set JWT_SECRET environment variable or AppSettings:Security:JwtSecret");
            }
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = appSettings.Security.JwtIssuer,
                ValidAudience = appSettings.Security.JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),
                NameClaimType = "name",
                RoleClaimType = "role",
                SaveSigninToken = true
            };
            
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Ensure all claims are preserved
                    var claims = context.Principal?.Claims.ToList() ?? new List<Claim>();
                    var identity = new ClaimsIdentity(claims, context.Scheme.Name);
                    context.Principal = new ClaimsPrincipal(identity);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddPolyBucketCors(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (isDevelopment)
                {
                    policy.WithOrigins(
                              "http://localhost:3001", 
                              "http://localhost:3002", 
                              "http://localhost:32768",
                              "http://polybucket-frontend:32768",
                              "http://polybucket-frontend:3000",
                              "http://127.0.0.1:32768",
                              "http://localhost:3000",
                              "http://localhost:10110",
                              "http://127.0.0.1:10110",
                              "http://marketplace-frontend:3000"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    var allowedOrigins = new List<string>();
                    
                    var corsOriginsFromConfig = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                    if (corsOriginsFromConfig != null && corsOriginsFromConfig.Length > 0)
                    {
                        allowedOrigins.AddRange(corsOriginsFromConfig);
                    }
                    
                    var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS__ALLOWED_ORIGINS");
                    if (!string.IsNullOrWhiteSpace(corsOriginsEnv))
                    {
                        var origins = corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        allowedOrigins.AddRange(origins);
                    }
                    
                    if (allowedOrigins.Count == 0)
                    {
                        throw new InvalidOperationException(
                            "CORS:AllowedOrigins must be configured in production environment via appsettings.json or CORS__ALLOWED_ORIGINS environment variable (comma-separated list)");
                    }

                    policy.WithOrigins(allowedOrigins.Distinct().ToArray())
                          .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                          .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                          .AllowCredentials();
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddPolyBucketHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection connection string is missing");
        }

        services.AddHealthChecks()
            .AddNpgSql(connectionString);

        return services;
    }

    public static IServiceCollection AddPolyBucketOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "v1";
            config.Title = "PolyBucket API";
            config.Version = "v1";
        });

        return services;
    }



    private static string GetJwtIssuer(IConfiguration configuration)
    {
        var jwtIssuer = configuration["AppSettings:Security:JwtIssuer"] 
            ?? configuration["JWT_ISSUER"]
            ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new InvalidOperationException(
                "JWT Issuer must be configured via AppSettings:Security:JwtIssuer, JWT_ISSUER environment variable, or appsettings.json");

        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            throw new InvalidOperationException(
                "JWT Issuer must be configured via AppSettings:Security:JwtIssuer, JWT_ISSUER environment variable, or appsettings.json");
        }

        return jwtIssuer;
    }

    private static string GetJwtAudience(IConfiguration configuration)
    {
        var jwtAudience = configuration["AppSettings:Security:JwtAudience"] 
            ?? configuration["JWT_AUDIENCE"]
            ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new InvalidOperationException(
                "JWT Audience must be configured via AppSettings:Security:JwtAudience, JWT_AUDIENCE environment variable, or appsettings.json");

        return jwtAudience;
    }
} 