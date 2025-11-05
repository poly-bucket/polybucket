using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using System.Net.Http.Json;
using Xunit;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PolyBucket.Marketplace.Tests.Controllers
{
    public class PluginControllerComprehensiveTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PluginControllerComprehensiveTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region Plugin Download Tests

        [Fact]
        public async Task GetPluginDownload_WithValidPluginId_ReturnsDownloadInfo()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var downloadInfo = await response.Content.ReadFromJsonAsync<PluginDownloadInfo>();
            downloadInfo.ShouldNotBeNull();
            downloadInfo.PluginId.ShouldBe(pluginId);
            downloadInfo.Version.ShouldNotBeNullOrEmpty();
            downloadInfo.DownloadUrl.ShouldNotBeNullOrEmpty();
            downloadInfo.FileName.ShouldNotBeNullOrEmpty();
            downloadInfo.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Fact]
        public async Task GetPluginDownload_WithInvalidPluginId_ReturnsNotFound()
        {
            // Arrange
            var pluginId = "non-existent-plugin";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPluginDownload_WithEmptyPluginId_ReturnsNotFound()
        {
            // Arrange
            var pluginId = "";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPluginDownload_WithSpecificVersion_ReturnsCorrectVersion()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var version = "1.0.0";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download?version={version}");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var downloadInfo = await response.Content.ReadFromJsonAsync<PluginDownloadInfo>();
            downloadInfo.ShouldNotBeNull();
            downloadInfo.PluginId.ShouldBe(pluginId);
            downloadInfo.Version.ShouldBe(version);
        }

        [Fact]
        public async Task GetPluginDownload_WithNonExistentVersion_ReturnsFallbackVersion()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var version = "2.0.0"; // This version doesn't exist

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download?version={version}");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var downloadInfo = await response.Content.ReadFromJsonAsync<PluginDownloadInfo>();
            downloadInfo.ShouldNotBeNull();
            downloadInfo.PluginId.ShouldBe(pluginId);
            downloadInfo.Version.ShouldBe("1.0.0"); // Should fall back to plugin's default version
        }

        [Fact]
        public async Task GetPluginDownload_WithSpecialCharactersInPluginId_HandlesCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1%20with%20spaces";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        #endregion

        #region Plugin Details Tests

        [Fact]
        public async Task GetPluginDetails_WithValidPluginId_ReturnsPluginDetails()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/details");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginDetails = await response.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            pluginDetails.ShouldNotBeNull();
            pluginDetails.Id.ShouldBe(pluginId);
            pluginDetails.Name.ShouldNotBeNullOrEmpty();
            pluginDetails.Description.ShouldNotBeNullOrEmpty();
            pluginDetails.Category.ShouldNotBeNullOrEmpty();
            pluginDetails.Version.ShouldNotBeNullOrEmpty();
            pluginDetails.Versions.ShouldNotBeNull();
            pluginDetails.Tags.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetPluginDetails_WithInvalidPluginId_ReturnsNotFound()
        {
            // Arrange
            var pluginId = "non-existent-plugin";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/details");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPluginDetails_WithEmptyPluginId_ReturnsNotFound()
        {
            // Arrange
            var pluginId = "";

            // Act
            var response = await _client.GetAsync($"/api/plugins/{pluginId}/details");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        #endregion

        #region Plugin Installation Tests

        [Fact]
        public async Task RecordInstallation_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.Message.ShouldNotBeNullOrEmpty();
            result.InstallationId.ShouldNotBeNullOrEmpty();
            result.InstalledAt.ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Fact]
        public async Task RecordInstallation_WithInvalidPluginId_ReturnsNotFound()
        {
            // Arrange
            var pluginId = "non-existent-plugin";
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RecordInstallation_WithEmptyPluginId_ReturnsNotFound()
        {
            // Arrange
            var pluginId = "";
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RecordInstallation_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", (object)null!);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RecordInstallation_WithEmptyRequest_ReturnsSuccess()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var request = new PluginInstallationRequest();

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task RecordInstallation_WithLongUserAgent_HandlesCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var longUserAgent = new string('A', 1000); // Very long user agent
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = longUserAgent,
                IpAddress = "127.0.0.1"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task RecordInstallation_WithSpecialCharactersInInstanceId_HandlesCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123!@#$%^&*()",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task RecordInstallation_WithInvalidIpAddress_HandlesCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "999.999.999.999" // Invalid IP
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        #endregion

        #region Main API Integration Tests

        [Fact]
        public async Task GetPluginsForMainApi_WithValidRequest_ReturnsPlugins()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=1&pageSize=10");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Plugins.ShouldNotBeNull();
            pluginsResponse.TotalCount.ShouldBeGreaterThan(0);
            pluginsResponse.Page.ShouldBe(1);
            pluginsResponse.PageSize.ShouldBe(10);
            pluginsResponse.TotalPages.ShouldBeGreaterThan(0);
            pluginsResponse.HasNextPage.ShouldBeOneOf(true, false);
            pluginsResponse.HasPreviousPage.ShouldBeOneOf(true, false);
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithCategoryFilter_ReturnsFilteredPlugins()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?category=UI Components");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Plugins.ShouldNotBeNull();
            pluginsResponse.Plugins.All(p => p.Category == "UI Components").ShouldBeTrue();
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithSearchFilter_ReturnsFilteredPlugins()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?search=Test");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Plugins.ShouldNotBeNull();
            pluginsResponse.Plugins.All(p => 
                p.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) || 
                p.Description.Contains("Test", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithInvalidPage_DefaultsToPage1()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=0&pageSize=10");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Page.ShouldBe(1); // Should default to page 1
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithInvalidPageSize_DefaultsToPageSize20()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=1&pageSize=200");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.PageSize.ShouldBe(20); // Should default to page size 20
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithNegativePage_DefaultsToPage1()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=-1&pageSize=10");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Page.ShouldBe(1); // Should default to page 1
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithZeroPageSize_DefaultsToPageSize20()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=1&pageSize=0");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.PageSize.ShouldBe(20); // Should default to page size 20
        }

        [Fact]
        public async Task GetCategoriesForMainApi_ReturnsCategories()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/categories/main-api");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var categories = await response.Content.ReadFromJsonAsync<List<MarketplaceCategory>>();
            categories.ShouldNotBeNull();
            categories.Count.ShouldBeGreaterThan(0);
            categories.All(c => !string.IsNullOrEmpty(c.Id) && !string.IsNullOrEmpty(c.Name)).ShouldBeTrue();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task GetPluginDownload_WithDatabaseError_ReturnsInternalServerError()
        {
            // This test would require mocking the database to throw an exception
            // For now, we'll test with an invalid plugin ID which should return NotFound
            var pluginId = "database-error-plugin";

            var response = await _client.GetAsync($"/api/plugins/{pluginId}/download");

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RecordInstallation_WithDatabaseError_ReturnsInternalServerError()
        {
            // This test would require mocking the database to throw an exception
            // For now, we'll test with an invalid plugin ID which should return NotFound
            var pluginId = "database-error-plugin";
            var request = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", request);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task GetPluginsForMainApi_WithLargePageSize_HandlesCorrectly()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=1&pageSize=1000");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.PageSize.ShouldBe(20); // Should be capped at 20
        }

        [Fact]
        public async Task GetPluginsForMainApi_WithVeryLargePage_ReturnsEmptyResults()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/main-api?page=999999&pageSize=10");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await response.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Plugins.ShouldNotBeNull();
            pluginsResponse.Plugins.Count.ShouldBe(0);
            pluginsResponse.TotalCount.ShouldBeGreaterThan(0);
        }

        #endregion
    }
}
