using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PolyBucket.Core.Configuration;
using PolyBucket.Core.Interfaces;
using PolyBucket.Infrastructure.Data;
using PolyBucket.Infrastructure.Services;
using PolyBucket.Infrastructure.Identity;
using PolyBucket.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PolyBucket.Api.Middleware;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;

namespace PolyBucket.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "Logs/polybucket-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Starting PolyBucket API");
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();

            // Configure AppSettings
            builder.Services.Configure<AppSettings>(builder.Configuration);

            // Add MVC controllers
            builder.Services.AddControllersWithViews();

            // Register infrastructure services - explicitly use the method from the InfrastructureServiceRegistration class
            PolyBucket.Infrastructure.InfrastructureServiceRegistration.AddInfrastructureServices(builder.Services, builder.Configuration);
            
            // Add authentication
            var appSettings = builder.Configuration.Get<AppSettings>();
            builder.Services.AddAuthentication(options =>
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
                        Encoding.UTF8.GetBytes(appSettings.Security.JwtSecret))
                };
            });

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PolyBucket API",
                    Version = "v1",
                    Description = "API for the PolyBucket 3D model repository"
                });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
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
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new string[] {}
                    }
                });
            });
            
            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Enable database migrations at startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<ApplicationDbContext>();
                    
                    // Log connection string (with password masked)
                    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
                    if (connectionString != null)
                    {
                        var maskedConnectionString = connectionString;
                        if (maskedConnectionString.Contains("Password="))
                        {
                            maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
                                maskedConnectionString,
                                @"Password=([^;]*)",
                                "Password=********"
                            );
                        }
                        app.Logger.LogInformation("Connection string: {ConnectionString}", maskedConnectionString);
                    }
                    
                    // Check if database exists and can be connected to
                    app.Logger.LogInformation("Checking database connection...");
                    bool canConnect = false;
                    int retryCount = 0;
                    int maxRetries = 5;
                    
                    while (!canConnect && retryCount < maxRetries)
                    {
                        try
                        {
                            canConnect = db.Database.CanConnect();
                            if (!canConnect)
                            {
                                retryCount++;
                                app.Logger.LogWarning("Cannot connect to database. Retry {RetryCount}/{MaxRetries} in 2 seconds...", retryCount, maxRetries);
                                System.Threading.Thread.Sleep(2000);
                            }
                        }
                        catch (Exception connEx)
                        {
                            retryCount++;
                            app.Logger.LogWarning(connEx, "Database connection error. Retry {RetryCount}/{MaxRetries} in 2 seconds...", retryCount, maxRetries);
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                    
                    if (!canConnect)
                    {
                        throw new Exception("Failed to connect to the database after multiple retries.");
                    }
                    
                    // Apply migrations
                    app.Logger.LogInformation("Attempting to apply migrations...");
                    try
                    {
                        db.Database.Migrate();
                        app.Logger.LogInformation("Migrations applied successfully.");
                    }
                    catch (Exception migrationEx)
                    {
                        app.Logger.LogError(migrationEx, "Error occurred while applying migrations");
                        
                        // Try using EnsureCreated as a fallback (for development scenarios)
                        if (app.Environment.IsDevelopment())
                        {
                            app.Logger.LogWarning("Attempting to use EnsureCreated as a fallback...");
                            db.Database.EnsureCreated();
                            app.Logger.LogInformation("Database schema created via EnsureCreated.");
                        }
                        else
                        {
                            throw; // Re-throw in production environments
                        }
                    }
                    
                    // Seed default roles
                    app.Logger.LogInformation("Seeding default roles...");
                    var roleSeedService = services.GetRequiredService<RoleSeedService>();
                    roleSeedService.SeedRolesAsync().GetAwaiter().GetResult();
                    app.Logger.LogInformation("Default roles seeded successfully.");
                    
                    // Initialize system setup if not already done
                    app.Logger.LogInformation("Initializing system setup...");
                    var systemSetupRepository = services.GetRequiredService<ISystemSetupRepository>();
                    var setupStatus = systemSetupRepository.GetSetupStatusAsync().GetAwaiter().GetResult();
                    app.Logger.LogInformation("System setup initialized. Admin configured: {AdminConfigured}, Roles configured: {RolesConfigured}",
                        setupStatus.IsAdminConfigured, setupStatus.IsRoleConfigured);
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "An error occurred while applying migrations or seeding data");
                    
                    if (app.Environment.IsDevelopment())
                    {
                        app.Logger.LogWarning("Starting application despite database initialization error (development mode)");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PolyBucket API v1");
                    c.RoutePrefix = string.Empty; // Serve the Swagger UI at the app's root
                });
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }
            
            // Move CORS middleware before routing and auth
            app.UseCors();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Add database test middleware
            app.UseDatabaseConnectionTest();
            
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
