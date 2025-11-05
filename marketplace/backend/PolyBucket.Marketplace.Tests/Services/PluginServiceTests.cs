using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Marketplace.Tests.Services
{
    public class PluginServiceTests : IDisposable
    {
        private readonly MarketplaceDbContext _context;
        private readonly Mock<ILogger<PluginService>> _mockLogger;
        private readonly PluginService _pluginService;

        public PluginServiceTests()
        {
            var options = new DbContextOptionsBuilder<MarketplaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MarketplaceDbContext(options);
            _mockLogger = new Mock<ILogger<PluginService>>();
            _pluginService = new PluginService(_context, _mockLogger.Object);

            SeedTestData();
        }

        [Fact]
        public async Task GetPluginDownloadAsync_WithValidPluginId_ReturnsDownloadInfo()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Act
            var result = await _pluginService.GetPluginDownloadAsync(pluginId);

            // Assert
            result.ShouldNotBeNull();
            result.PluginId.ShouldBe(pluginId);
            result.Version.ShouldNotBeNullOrEmpty();
            result.DownloadUrl.ShouldNotBeNullOrEmpty();
            result.FileName.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetPluginDownloadAsync_WithInvalidPluginId_ReturnsNull()
        {
            // Arrange
            var pluginId = "non-existent-plugin";

            // Act
            var result = await _pluginService.GetPluginDownloadAsync(pluginId);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetPluginDownloadAsync_WithSpecificVersion_ReturnsCorrectVersion()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var version = "1.0.0";

            // Act
            var result = await _pluginService.GetPluginDownloadAsync(pluginId, version);

            // Assert
            result.ShouldNotBeNull();
            result.PluginId.ShouldBe(pluginId);
            result.Version.ShouldBe(version);
        }

        [Fact]
        public async Task GetPluginDownloadAsync_WithNonExistentVersion_ReturnsFallbackVersion()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var version = "2.0.0"; // This version doesn't exist

            // Act
            var result = await _pluginService.GetPluginDownloadAsync(pluginId, version);

            // Assert
            result.ShouldNotBeNull();
            result.PluginId.ShouldBe(pluginId);
            result.Version.ShouldBe("1.0.0"); // Should fall back to plugin's default version
        }

        [Fact]
        public async Task GetPluginDetailsAsync_WithValidPluginId_ReturnsPluginDetails()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Act
            var result = await _pluginService.GetPluginDetailsAsync(pluginId);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(pluginId);
            result.Name.ShouldNotBeNullOrEmpty();
            result.Description.ShouldNotBeNullOrEmpty();
            result.Category.ShouldNotBeNullOrEmpty();
            result.Version.ShouldNotBeNullOrEmpty();
            result.Versions.ShouldNotBeNull();
            result.Tags.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetPluginDetailsAsync_WithInvalidPluginId_ReturnsNull()
        {
            // Arrange
            var pluginId = "non-existent-plugin";

            // Act
            var result = await _pluginService.GetPluginDetailsAsync(pluginId);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task RecordInstallationAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new PluginInstallationRequest
            {
                PluginId = "test-plugin-1",
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            var initialDownloads = _context.Plugins.Find("test-plugin-1")!.Downloads;

            // Act
            var result = await _pluginService.RecordInstallationAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.Message.ShouldNotBeNullOrEmpty();
            result.InstallationId.ShouldNotBeNullOrEmpty();
            result.InstalledAt.ShouldBeGreaterThan(DateTime.MinValue);

            // Verify download count was incremented
            var plugin = _context.Plugins.Find("test-plugin-1");
            plugin!.Downloads.ShouldBe(initialDownloads + 1);

            // Verify installation record was created
            var installation = _context.Downloads.FirstOrDefault(d => d.PluginId == "test-plugin-1");
            installation.ShouldNotBeNull();
            installation.InstanceId.ShouldBe("test-instance-123");
            installation.UserAgent.ShouldBe("Test User Agent");
            installation.IpAddress.ShouldBe("127.0.0.1");
        }

        [Fact]
        public async Task RecordInstallationAsync_WithInvalidPluginId_ReturnsFailure()
        {
            // Arrange
            var request = new PluginInstallationRequest
            {
                PluginId = "non-existent-plugin",
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var result = await _pluginService.RecordInstallationAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Plugin not found");
            result.InstallationId.ShouldBeNull();
        }

        [Fact]
        public async Task RecordInstallationAsync_WithNullPluginId_ReturnsFailure()
        {
            // Arrange
            var request = new PluginInstallationRequest
            {
                PluginId = null!,
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var result = await _pluginService.RecordInstallationAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Plugin not found");
        }

        [Fact]
        public async Task RecordInstallationAsync_WithEmptyPluginId_ReturnsFailure()
        {
            // Arrange
            var request = new PluginInstallationRequest
            {
                PluginId = "",
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var result = await _pluginService.RecordInstallationAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Plugin not found");
        }

        [Fact]
        public async Task RecordInstallationAsync_WithDatabaseException_ReturnsFailure()
        {
            // Arrange
            var request = new PluginInstallationRequest
            {
                PluginId = "test-plugin-1",
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Dispose context to simulate database error
            _context.Dispose();

            // Act
            var result = await _pluginService.RecordInstallationAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to record installation");
        }

        [Fact]
        public async Task GetPluginsForMainApiAsync_WithValidParameters_ReturnsPlugins()
        {
            // Act
            var result = await _pluginService.GetPluginsForMainApiAsync(page: 1, pageSize: 10);

            // Assert
            result.ShouldNotBeNull();
            result.Plugins.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Page.ShouldBe(1);
            result.PageSize.ShouldBe(10);
            result.TotalPages.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetPluginsForMainApiAsync_WithCategoryFilter_ReturnsFilteredPlugins()
        {
            // Act
            var result = await _pluginService.GetPluginsForMainApiAsync(
                page: 1, 
                pageSize: 10, 
                category: "UI Components");

            // Assert
            result.ShouldNotBeNull();
            result.Plugins.ShouldNotBeNull();
            result.Plugins.All(p => p.Category == "UI Components").ShouldBeTrue();
        }

        [Fact]
        public async Task GetPluginsForMainApiAsync_WithSearchFilter_ReturnsFilteredPlugins()
        {
            // Act
            var result = await _pluginService.GetPluginsForMainApiAsync(
                page: 1, 
                pageSize: 10, 
                search: "Test");

            // Assert
            result.ShouldNotBeNull();
            result.Plugins.ShouldNotBeNull();
            result.Plugins.All(p => 
                p.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) || 
                p.Description.Contains("Test", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        }

        [Fact]
        public async Task GetCategoriesForMainApiAsync_ReturnsCategories()
        {
            // Act
            var result = await _pluginService.GetCategoriesForMainApiAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBeGreaterThan(0);
            result.All(c => !string.IsNullOrEmpty(c.Id) && !string.IsNullOrEmpty(c.Name)).ShouldBeTrue();
        }

        [Fact]
        public async Task BrowsePluginsAsync_WithValidRequest_ReturnsPlugins()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                Search = "Test",
                Page = 1,
                PageSize = 10,
                SortBy = "downloads",
                SortOrder = "desc"
            };

            // Act
            var result = await _pluginService.BrowsePluginsAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Plugins.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Page.ShouldBe(1);
            result.PageSize.ShouldBe(10);
            result.TotalPages.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task BrowsePluginsAsync_WithInvalidPage_ReturnsActualPage()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                Page = 0, // Invalid page
                PageSize = 10
            };

            // Act
            var result = await _pluginService.BrowsePluginsAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Page.ShouldBe(0); // Service doesn't validate, returns actual value
        }

        [Fact]
        public async Task BrowsePluginsAsync_WithInvalidPageSize_ReturnsActualPageSize()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                Page = 1,
                PageSize = 200 // Invalid page size (too large)
            };

            // Act
            var result = await _pluginService.BrowsePluginsAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.PageSize.ShouldBe(200); // Service doesn't validate, returns actual value
        }

        [Fact]
        public async Task BrowsePluginsAsync_WithSorting_ReturnsSortedPlugins()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                SortBy = "rating",
                SortOrder = "desc",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _pluginService.BrowsePluginsAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Plugins.ShouldNotBeNull();
            
            // Verify plugins are sorted by rating in descending order
            for (int i = 0; i < result.Plugins.Count - 1; i++)
            {
                result.Plugins[i].AverageRating.ShouldBeGreaterThanOrEqualTo(result.Plugins[i + 1].AverageRating);
            }
        }

        private void SeedTestData()
        {
            var testPlugins = new List<Plugin>
            {
                new Plugin
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
                    Versions = new List<PluginVersion>
                    {
                        new PluginVersion
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
                new Plugin
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
                    Versions = new List<PluginVersion>
                    {
                        new PluginVersion
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
                new Plugin
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
                    Versions = new List<PluginVersion>
                    {
                        new PluginVersion
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

            _context.Plugins.AddRange(testPlugins);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
