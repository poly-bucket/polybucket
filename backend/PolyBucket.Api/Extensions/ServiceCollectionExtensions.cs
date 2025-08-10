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
                    Encoding.UTF8.GetBytes(appSettings.Security.JwtSecret)),
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

    public static IServiceCollection AddPolyBucketCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                          "http://localhost:3001", 
                          "http://localhost:3002", 
                          "http://localhost:32768",
                          "http://host.docker.internal:32768",
                          "http://polybucket-client-1:3000",
                          "http://127.0.0.1:32768",
                          "http://localhost:3000",
                          "http://localhost:11666"
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
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
} 