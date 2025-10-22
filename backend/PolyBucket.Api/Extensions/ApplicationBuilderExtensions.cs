using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Data.Seeders;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Seeders;
using PolyBucket.Api.Features.ThemeManagement.Domain;

namespace PolyBucket.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<WebApplication> ConfigurePolyBucketApplicationAsync(this WebApplication app)
    {
        await InitializeDatabaseAsync(app);
        ConfigureMiddleware(app);
        ConfigureStaticFiles(app);
        ConfigureEndpoints(app);

        return app;
    }

    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var db = services.GetRequiredService<PolyBucketDbContext>();
            
            // Skip migrations in test environment
            if (!app.Environment.IsEnvironment("Test"))
            {
                db.Database.Migrate();
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
            app.Logger.LogError(ex, "An error occurred while applying migrations or seeding data");
        }
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.UseCors();
        app.UseStaticFiles();
    }

    private static void ConfigureStaticFiles(WebApplication app)
    {
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
    }

    private static void ConfigureEndpoints(WebApplication app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
    }
} 