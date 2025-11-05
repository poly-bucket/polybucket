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
    public class PluginInstallationIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PluginInstallationIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

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
            pluginsResponse.Plugins.All(p => p.Name.Contains("Test") || p.Description.Contains("Test")).ShouldBeTrue();
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

    }
}
