using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PolyBucket.Api.Middleware;
using Serilog;
using Serilog.Events;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Extensions.Serilog;
using Microsoft.Extensions.FileProviders;
using PolyBucket.Api.Data;
using PolyBucket.Api.Data.Seeders;

namespace PolyBucket.Api;

public class AppSettings
{
    public SecuritySettings Security { get; set; } = new();
}

public class SecuritySettings
{
    public string JwtIssuer { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
#if DEBUG
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: ConsoleTheme.CustomTheme)
#endif
            .WriteTo.File(
                path: "Logs/.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LoggingExtensions.SetLogLevel(builder.Configuration["Logging:LogLevel:Default"] ?? "Information"),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Starting PolyBucket API");
            
            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddAuthorization();

            builder.Services.Configure<AppSettings>(builder.Configuration);

            builder.Services.AddDbContext<PolyBucketDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddFeatures();
            builder.Services.AddPlugins();

            builder.Services.AddTransient<AdminSeeder>();
            builder.Services.AddTransient<ModelSeeder>();

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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PolyBucket API", Version = "v1" });
            });
            
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

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<PolyBucketDbContext>();
                    db.Database.Migrate();
                    
                    var adminSeeder = services.GetRequiredService<AdminSeeder>();
                    await adminSeeder.SeedAsync();

                    var modelSeeder = services.GetRequiredService<ModelSeeder>();
                    await modelSeeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "An error occurred while applying migrations or seeding data");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseStaticFiles();

            var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "files");
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(filesPath),
                RequestPath = "/files"
            });
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
