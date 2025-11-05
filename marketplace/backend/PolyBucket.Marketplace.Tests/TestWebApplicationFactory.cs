using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using PolyBucket.Marketplace.Api.Data;

namespace PolyBucket.Marketplace.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string DatabaseName = $"TestDatabase_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MarketplaceDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing with a shared name
                services.AddDbContext<MarketplaceDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                    options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to get the DbContext and seed data
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
                    context.Database.EnsureCreated();
                    SeedTestData(context);
                }
            });
        }

        private static void SeedTestData(MarketplaceDbContext context)
        {
            // Clear existing data first to avoid duplicates
            context.Plugins.RemoveRange(context.Plugins);
            context.SaveChanges();

            var testPlugins = new List<PolyBucket.Marketplace.Api.Models.Plugin>
            {
                new PolyBucket.Marketplace.Api.Models.Plugin
                {
                    Id = "test-plugin-1",
                    Name = "Test Plugin 1",
                    Description = "A test plugin for testing",
                    Category = "UI Components",
                    Version = "1.0.0",
                    AuthorId = "test-author-1",
                    Downloads = 100,
                    AverageRating = 4.5,
                    IsVerified = true,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    Author = null, // Don't set navigation property for in-memory testing
                    Versions = new List<PolyBucket.Marketplace.Api.Models.PluginVersion>
                    {
                        new PolyBucket.Marketplace.Api.Models.PluginVersion
                        {
                            Id = "version-1-1",
                            PluginId = "test-plugin-1",
                            Version = "1.0.0",
                            DownloadUrl = "/api/plugins/test-plugin-1/download",
                            ReleaseNotes = "Initial release",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        }
                    }
                },
                new PolyBucket.Marketplace.Api.Models.Plugin
                {
                    Id = "test-plugin-2",
                    Name = "Test Plugin 2",
                    Description = "Another test plugin",
                    Category = "Authentication",
                    Version = "2.0.0",
                    AuthorId = "test-author-2",
                    Downloads = 200,
                    AverageRating = 4.8,
                    IsVerified = false,
                    IsFeatured = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    Author = null, // Don't set navigation property for in-memory testing
                    Versions = new List<PolyBucket.Marketplace.Api.Models.PluginVersion>
                    {
                        new PolyBucket.Marketplace.Api.Models.PluginVersion
                        {
                            Id = "version-2-1",
                            PluginId = "test-plugin-2",
                            Version = "2.0.0",
                            DownloadUrl = "/api/plugins/test-plugin-2/download",
                            ReleaseNotes = "Major update",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-5)
                        }
                    }
                },
                new PolyBucket.Marketplace.Api.Models.Plugin
                {
                    Id = "test-plugin-3",
                    Name = "Test Plugin 3",
                    Description = "Third test plugin",
                    Category = "Data Visualization",
                    Version = "1.5.0",
                    AuthorId = "test-author-3",
                    Downloads = 50,
                    AverageRating = 3.9,
                    IsVerified = false,
                    IsFeatured = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow,
                    Author = null, // Don't set navigation property for in-memory testing
                    Versions = new List<PolyBucket.Marketplace.Api.Models.PluginVersion>
                    {
                        new PolyBucket.Marketplace.Api.Models.PluginVersion
                        {
                            Id = "version-3-1",
                            PluginId = "test-plugin-3",
                            Version = "1.5.0",
                            DownloadUrl = "/api/plugins/test-plugin-3/download",
                            ReleaseNotes = "Bug fixes",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-2)
                        }
                    }
                }
            };

            context.Plugins.AddRange(testPlugins);
            context.SaveChanges();
        }
    }
}
