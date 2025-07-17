using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using PolyBucket.Api.Middleware;
using Serilog;
using Serilog.Events;
using PolyBucket.Api.Extensions.Serilog;
using Microsoft.Extensions.FileProviders;
using PolyBucket.Api.Data;
using PolyBucket.Api.Data.Seeders;
using MediatR;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Settings;
using PolyBucket.Api.Extensions;

namespace PolyBucket.Api;

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
            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddDbContext<PolyBucketDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    
            // Models
            builder.Services.AddTransient<Features.Models.Repository.IModelsRepository, Features.Models.Repository.ModelsRepository>();
            
            // Model Previews
            builder.Services.AddTransient<Features.Models.GetModelPreview.Repository.IModelPreviewRepository, Features.Models.GetModelPreview.Repository.ModelPreviewRepository>();
            builder.Services.AddTransient<Features.Models.GenerateModelPreview.Repository.IGenerateModelPreviewRepository, Features.Models.GenerateModelPreview.Repository.GenerateModelPreviewRepository>();

            // Collections
            builder.Services.AddTransient<Features.Collections.CreateCollection.Repository.ICollectionRepository, Features.Collections.CreateCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.GetCollectionById.Repository.ICollectionRepository, Features.Collections.GetCollectionById.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.UpdateCollection.Repository.ICollectionRepository, Features.Collections.UpdateCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.DeleteCollection.Repository.ICollectionRepository, Features.Collections.DeleteCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.GetUserCollections.Repository.ICollectionRepository, Features.Collections.GetUserCollections.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.AddModelToCollection.Repository.ICollectionRepository, Features.Collections.AddModelToCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.RemoveModelFromCollection.Repository.ICollectionRepository, Features.Collections.RemoveModelFromCollection.Repository.CollectionRepository>();

            // Users
            builder.Services.AddTransient<Features.Users.Repository.IUserRepository, Features.Users.Repository.UserRepository>();
            
            // Authentication
            builder.Services.AddTransient<Features.Authentication.Repository.IAuthenticationRepository, Features.Authentication.Repository.AuthenticationRepository>();
            builder.Services.AddTransient<Features.Authentication.Services.ITokenService, Features.Authentication.Services.TokenService>();
            builder.Services.AddTransient<Features.Authentication.Services.IEmailService, Features.Authentication.Services.EmailService>();
            builder.Services.AddTransient<Features.Authentication.Register.Domain.RegisterCommandHandler>();
            
            // ACL Services
            builder.Services.AddTransient<Features.ACL.Services.IPermissionService, Features.ACL.Services.PermissionService>();

            builder.Services.AddSingleton<PolyBucket.Api.Common.Plugins.PluginManager>(provider =>
            {
                var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
                return new PolyBucket.Api.Common.Plugins.PluginManager(pluginsPath);
            });

            // Register default plugins
            builder.Services.AddScoped<PolyBucket.Api.Features.Comments.Domain.ICommentsPlugin, PolyBucket.Api.Features.Comments.Plugins.DefaultCommentsPlugin>();

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IPasswordGenerator, PasswordGenerator>();
            builder.Services.AddTransient<Features.Users.CreateUser.Domain.CreateUserCommandHandler>();

            // Email Settings
            builder.Services.AddTransient<Features.SystemSettings.Domain.UpdateEmailSettingsCommandHandler>();
            builder.Services.AddTransient<Features.SystemSettings.Domain.TestEmailConfigurationCommandHandler>();

            builder.Services.AddTransient<AdminSeeder>();
            builder.Services.AddTransient<ModelSeeder>();
            builder.Services.AddTransient<CollectionSeeder>();

            builder.Services.AddObjectStorage(builder.Configuration);

            var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
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
            builder.Services.AddOpenApiDocument(config =>
            {
                config.DocumentName = "v1";
                config.Title = "PolyBucket API";
                config.Version = "v1";
            });
            
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:3001", 
                              "http://localhost:3002", 
                              "http://localhost:32768",
                              "http://host.docker.internal:32768",
                              "http://polybucket-client-1:3000",
                              "http://127.0.0.1:32768"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));

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

                    var collectionSeeder = services.GetRequiredService<CollectionSeeder>();
                    await collectionSeeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "An error occurred while applying migrations or seeding data");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi();
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
            app.MapHealthChecks("/health");

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
