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
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.Plugins;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.SystemSettings.Plugins;

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

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddDbContext<PolyBucketDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    
            // Models
            builder.Services.AddTransient<Features.Models.Repository.IModelsRepository, Features.Models.Repository.ModelsRepository>();
            
            // CreateModel
            builder.Services.AddTransient<Features.Models.CreateModel.Domain.ICreateModelService, Features.Models.CreateModel.Domain.CreateModelService>();
            builder.Services.AddTransient<Features.Models.CreateModel.Repository.ICreateModelRepository, Features.Models.CreateModel.Repository.CreateModelRepository>();
            
            // CreateModelVersion
            builder.Services.AddTransient<Features.Models.CreateModelVersion.Domain.ICreateModelVersionService, Features.Models.CreateModelVersion.Domain.CreateModelVersionService>();
            builder.Services.AddTransient<Features.Models.CreateModelVersion.Repository.ICreateModelVersionRepository, Features.Models.CreateModelVersion.Repository.CreateModelVersionRepository>();
            
            // UpdateModel
            builder.Services.AddTransient<Features.Models.UpdateModel.Domain.IUpdateModelService, Features.Models.UpdateModel.Domain.UpdateModelService>();
            builder.Services.AddTransient<Features.Models.UpdateModel.Repository.IUpdateModelRepository, Features.Models.UpdateModel.Repository.UpdateModelRepository>();
            
            // UpdateModelVersion
            builder.Services.AddTransient<Features.Models.UpdateModelVersion.Domain.IUpdateModelVersionService, Features.Models.UpdateModelVersion.Domain.UpdateModelVersionService>();
            builder.Services.AddTransient<Features.Models.UpdateModelVersion.Repository.IUpdateModelVersionRepository, Features.Models.UpdateModelVersion.Repository.UpdateModelVersionRepository>();
            
            // DeleteModel
            builder.Services.AddTransient<Features.Models.DeleteModel.Domain.IDeleteModelService, Features.Models.DeleteModel.Domain.DeleteModelService>();
            builder.Services.AddTransient<Features.Models.DeleteModel.Repository.IDeleteModelRepository, Features.Models.DeleteModel.Repository.DeleteModelRepository>();
            
            // DeleteModelVersion
            builder.Services.AddTransient<Features.Models.DeleteModelVersion.Domain.IDeleteModelVersionService, Features.Models.DeleteModelVersion.Domain.DeleteModelVersionService>();
            builder.Services.AddTransient<Features.Models.DeleteModelVersion.Repository.IDeleteModelVersionRepository, Features.Models.DeleteModelVersion.Repository.DeleteModelVersionRepository>();
            
            // GetModelByUserId
            builder.Services.AddTransient<Features.Models.GetModelByUserId.Domain.IGetModelByUserIdService, Features.Models.GetModelByUserId.Domain.GetModelByUserIdService>();
            builder.Services.AddTransient<Features.Models.GetModelByUserId.Repository.IGetModelByUserIdRepository, Features.Models.GetModelByUserId.Repository.GetModelByUserIdRepository>();
            
            // Model Previews
            builder.Services.AddTransient<Features.Models.GetModelPreview.Repository.IModelPreviewRepository, Features.Models.GetModelPreview.Repository.ModelPreviewRepository>();
            builder.Services.AddTransient<Features.Models.GenerateModelPreview.Repository.IGenerateModelPreviewRepository, Features.Models.GenerateModelPreview.Repository.GenerateModelPreviewRepository>();
            builder.Services.AddTransient<Features.Models.GenerateModelPreview.Services.IModelPreviewGenerationService, Features.Models.GenerateModelPreview.Services.ModelPreviewGenerationService>();

            // Collections
            builder.Services.AddTransient<Features.Collections.CreateCollection.Repository.ICollectionRepository, Features.Collections.CreateCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.GetCollectionById.Repository.ICollectionRepository, Features.Collections.GetCollectionById.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.UpdateCollection.Repository.ICollectionRepository, Features.Collections.UpdateCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.DeleteCollection.Repository.ICollectionRepository, Features.Collections.DeleteCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.GetUserCollections.Repository.ICollectionRepository, Features.Collections.GetUserCollections.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.AddModelToCollection.Repository.ICollectionRepository, Features.Collections.AddModelToCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.RemoveModelFromCollection.Repository.ICollectionRepository, Features.Collections.RemoveModelFromCollection.Repository.CollectionRepository>();
            builder.Services.AddTransient<Features.Collections.AccessCollection.Repository.ICollectionRepository, Features.Collections.AccessCollection.Repository.CollectionRepository>();
            
            // Collection Command Handlers
            builder.Services.AddTransient<Features.Collections.CreateCollection.Domain.CreateCollectionCommandHandler>();
            builder.Services.AddTransient<Features.Collections.UpdateCollection.Domain.UpdateCollectionCommandHandler>();
            builder.Services.AddTransient<Features.Collections.DeleteCollection.Domain.DeleteCollectionCommandHandler>();
            builder.Services.AddTransient<Features.Collections.GetCollectionById.Domain.GetCollectionByIdQueryHandler>();
            builder.Services.AddTransient<Features.Collections.GetUserCollections.Domain.GetUserCollectionsQueryHandler>();
            builder.Services.AddTransient<Features.Collections.AddModelToCollection.Domain.AddModelToCollectionCommandHandler>();
            builder.Services.AddTransient<Features.Collections.RemoveModelFromCollection.Domain.RemoveModelFromCollectionCommandHandler>();
            builder.Services.AddTransient<Features.Collections.AccessCollection.Domain.AccessCollectionCommandHandler>();

            // Users
            builder.Services.AddTransient<Features.Users.Repository.IUserRepository, Features.Users.Repository.UserRepository>();
            
            // Authentication
            builder.Services.AddTransient<Features.Authentication.Repository.IAuthenticationRepository, Features.Authentication.Repository.AuthenticationRepository>();
            builder.Services.AddTransient<Features.Authentication.Services.ITokenService, Features.Authentication.Services.TokenService>();
            builder.Services.AddTransient<Features.Authentication.Services.IEmailService, Features.Authentication.Services.EmailService>();
            builder.Services.AddTransient<Features.Authentication.Register.Domain.RegisterCommandHandler>();
            builder.Services.AddTransient<Features.Authentication.Login.Domain.LoginCommandHandler>();
            builder.Services.AddTransient<Features.Authentication.ChangePassword.Domain.ChangePasswordCommandHandler>();
            
            // System Settings
            builder.Services.AddTransient<Features.SystemSettings.Services.IAuthenticationSettingsService, Features.SystemSettings.Services.AuthenticationSettingsService>();
            builder.Services.AddTransient<Features.SystemSettings.Services.ITokenSettingsService, Features.SystemSettings.Services.TokenSettingsService>();
            
            // ACL Services
            builder.Services.AddTransient<Features.ACL.Services.IPermissionService, Features.ACL.Services.PermissionService>();
            
            // Avatar Service
            builder.Services.AddTransient<Common.Services.IAvatarService, Common.Services.AvatarService>();
            
            // Regenerate Avatar Service
            builder.Services.AddTransient<Features.Users.RegenerateAvatar.Repository.IRegenerateAvatarRepository, Features.Users.RegenerateAvatar.Repository.RegenerateAvatarRepository>();
            builder.Services.AddTransient<Features.Users.RegenerateAvatar.Domain.IRegenerateAvatarService, Features.Users.RegenerateAvatar.Domain.RegenerateAvatarService>();
            
            // User Settings Services
            builder.Services.AddTransient<Features.Users.UpdateUserSettings.Handlers.UpdateUserSettingsCommandHandler>();
            builder.Services.AddTransient<Features.Users.Queries.GetUserSettingsQueryHandler>();

            // Authorization Filters
            builder.Services.AddScoped<Features.Files.Http.PublicModelAuthorizationFilter>();

            builder.Services.AddSingleton<PolyBucket.Api.Common.Plugins.PluginManager>(provider =>
            {
                var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
                return new PolyBucket.Api.Common.Plugins.PluginManager(pluginsPath);
            });

            // Register default plugins
            builder.Services.AddScoped<PolyBucket.Api.Features.Comments.Domain.ICommentsPlugin, PolyBucket.Api.Features.Comments.Plugins.DefaultCommentsPlugin>();
            builder.Services.AddScoped<PolyBucket.Api.Features.Reports.Domain.IReportingPlugin, PolyBucket.Api.Features.Reports.Plugins.DefaultReportingPlugin>();
            builder.Services.AddScoped<PolyBucket.Api.Features.Reports.Repository.IReportsRepository, PolyBucket.Api.Features.Reports.Repository.ReportsRepository>();
            
            // Theme System
            builder.Services.AddScoped<PolyBucket.Api.Common.Plugins.IThemePlugin, PolyBucket.Api.Features.SystemSettings.Plugins.LiquidGlassThemePlugin>();
            builder.Services.AddScoped<PolyBucket.Api.Common.Services.IThemeManager, PolyBucket.Api.Common.Services.ThemeManager>();

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IPasswordGenerator, PasswordGenerator>();
            builder.Services.AddTransient<Features.Users.CreateUser.Domain.CreateUserCommandHandler>();

            // Email Settings
            builder.Services.AddTransient<Features.SystemSettings.Domain.UpdateEmailSettingsCommandHandler>();
            builder.Services.AddTransient<Features.SystemSettings.Domain.TestEmailConfigurationCommandHandler>();
            
            // System Setup
            builder.Services.AddTransient<Features.SystemSettings.CheckFirstTimeSetup.Domain.CheckFirstTimeSetupQueryHandler>();
            builder.Services.AddTransient<Features.SystemSettings.UpdateSiteSettings.Domain.UpdateSiteSettingsCommandHandler>();
            builder.Services.AddTransient<Features.SystemSettings.CompleteFirstTimeSetup.Domain.CompleteFirstTimeSetupCommandHandler>();

            builder.Services.AddTransient<AdminSeeder>();
            builder.Services.AddTransient<ModelSeeder>();
            // builder.Services.AddTransient<CollectionSeeder>();

            builder.Services.AddObjectStorage(builder.Configuration);

            var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
            if (appSettings?.Security == null)
            {
                throw new InvalidOperationException("AppSettings:Security configuration is missing");
            }

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection connection string is missing");
            }

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
                              "http://127.0.0.1:32768",
                              "http://localhost:3000",
                              "http://localhost:11666"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddHealthChecks()
                .AddNpgSql(connectionString);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<PolyBucketDbContext>();
                    
                    // Skip migrations in test environment
                    if (!app.Environment.IsEnvironment("Test"))
                    {
                        db.Database.Migrate();
                    }
                    
                    // Initialize default roles first
                    var permissionService = services.GetRequiredService<Features.ACL.Services.IPermissionService>();
                    await permissionService.InitializeDefaultRolesAsync();
                    
                    var adminSeeder = services.GetRequiredService<AdminSeeder>();
                    await adminSeeder.SeedAsync();

                    // Clear existing seeded data - already done in previous run

                    var modelSeeder = services.GetRequiredService<ModelSeeder>();
                    await modelSeeder.SeedAsync();

                    // var collectionSeeder = services.GetRequiredService<CollectionSeeder>();
                    // await collectionSeeder.SeedAsync();
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
