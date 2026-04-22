using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Data.Seeders;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Seeders;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Middleware;

namespace PolyBucket.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<WebApplication> ConfigurePolyBucketApplicationAsync(this WebApplication app)
    {
        await InitializeDatabaseAsync(app);
        ConfigureMiddleware(app);

        return app;
    }

    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        if (app.Configuration.GetValue("Database:SkipHostDatabaseInitialization", false))
        {
            app.Logger.LogInformation(
                "Skipping host database initialization (Database:SkipHostDatabaseInitialization). The test pipeline owns ensure, migrate, and seed.");
            return;
        }

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var cancellationToken = app.Lifetime.ApplicationStopping;
        
        try
        {
            var db = services.GetRequiredService<PolyBucketDbContext>();
            if (!app.Environment.IsEnvironment("Test"))
            {
                var ensurer = new PostgresAppDatabaseEnsurer(app.Configuration);
                await ensurer.EnsureAppDatabaseExistsOrValidateForMigrationAsync(app.Logger, cancellationToken);
                await db.Database.MigrateAsync(cancellationToken);
                app.Logger.LogInformation("Database migrations applied successfully");
            }
            
            // Initialize default permissions and roles first
            var permissionService = services.GetRequiredService<IPermissionService>();
            await permissionService.InitializeDefaultPermissionsAsync();
            await permissionService.InitializeDefaultRolesAsync();
            
            var adminSeeder = services.GetRequiredService<AdminSeeder>();
            await adminSeeder.SeedAsync();
            
            // Seed categories
            var categorySeeder = services.GetRequiredService<CategorySeeder>();
            await categorySeeder.SeedAsync();
            
            // Seed themes
            await ThemeSeeder.SeedThemesAsync(db);
            
            // Seed file type settings
            var fileTypeSettingsSeeder = services.GetRequiredService<FileTypeSettingsSeeder>();
            await fileTypeSettingsSeeder.SeedAsync();
            
            // Seed model settings
            var modelSettingsSeeder = services.GetRequiredService<ModelSettingsSeeder>();
            await modelSettingsSeeder.SeedAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while applying migrations or seeding data. Startup is aborted.");
            throw;
        }
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseGlobalExceptionHandler();
        
        app.UseCors();
        
        app.UseRequestLogging();
        
        app.UseSecurityHeaders();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "files");
        if (!Directory.Exists(filesPath))
        {
            Directory.CreateDirectory(filesPath);
        }
        
        app.UseStaticFiles();
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(filesPath),
            RequestPath = "/files"
        });
        
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapHealthChecks("/health");
        
        app.MapControllers();
    }
} 