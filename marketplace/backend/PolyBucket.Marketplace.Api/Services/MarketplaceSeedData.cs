using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services;

public static class MarketplaceSeedData
{
    private static readonly (string Id, string Name, string Description, string Category, string Version, string Subpath, string Targets)[] DefaultPlugins =
    [
        ("dark-theme-plugin", "Dark Theme", "Dark theme with customizable accent colors", "Themes", "1.2.0", "examples/plugins/dark-theme-plugin", "frontend"),
        ("sample-api-plugin", "Sample API Plugin", "Minimal API plugin for install tests", "Integrations", "1.0.0", "examples/plugins/sample-api-plugin", "api"),
        ("sample-dual-plugin", "Sample Dual Plugin", "API and frontend assets for install tests", "Integrations", "1.0.0", "examples/plugins/sample-dual-plugin", "api,frontend")
    ];

    public static async Task SeedAsync(MarketplaceDbContext context, IConfiguration configuration, ILogger logger)
    {
        if (await context.Plugins.AnyAsync())
        {
            return;
        }

        var repositoryUrl = configuration["Marketplace:SeedRepositoryUrl"]
            ?? "https://github.com/poly-bucket/polybucket";
        var branch = configuration["Marketplace:SeedRepositoryBranch"] ?? "main";

        foreach (var item in DefaultPlugins)
        {
            context.Plugins.Add(new Plugin
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                LongDescription = item.Description,
                Category = item.Category,
                Version = item.Version,
                RepositoryUrl = repositoryUrl,
                RepositoryBranch = branch,
                RepositorySubpath = item.Subpath,
                Targets = item.Targets,
                License = "MIT",
                Status = "published",
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} marketplace plugins", DefaultPlugins.Length);
    }
}
