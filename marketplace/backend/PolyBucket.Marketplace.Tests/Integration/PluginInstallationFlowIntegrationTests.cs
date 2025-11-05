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

namespace PolyBucket.Marketplace.Tests.Integration
{
    public class PluginInstallationFlowIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PluginInstallationFlowIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CompleteInstallationFlow_WithValidPlugin_ShouldSucceed()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Step 1: Get plugin details
            var detailsResponse = await _client.GetAsync($"/api/plugins/{pluginId}/details");
            detailsResponse.IsSuccessStatusCode.ShouldBeTrue();
            var pluginDetails = await detailsResponse.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            pluginDetails.ShouldNotBeNull();
            pluginDetails.Id.ShouldBe(pluginId);

            // Step 2: Get download information
            var downloadResponse = await _client.GetAsync($"/api/plugins/{pluginId}/download");
            downloadResponse.IsSuccessStatusCode.ShouldBeTrue();
            var downloadInfo = await downloadResponse.Content.ReadFromJsonAsync<PluginDownloadInfo>();
            downloadInfo.ShouldNotBeNull();
            downloadInfo.PluginId.ShouldBe(pluginId);

            // Step 3: Record installation
            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);
            installResponse.IsSuccessStatusCode.ShouldBeTrue();
            var installResult = await installResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            installResult.ShouldNotBeNull();
            installResult.Success.ShouldBeTrue();
            installResult.InstallationId.ShouldNotBeNullOrEmpty();

            // Step 4: Verify installation was recorded
            var updatedDetailsResponse = await _client.GetAsync($"/api/plugins/{pluginId}/details");
            updatedDetailsResponse.IsSuccessStatusCode.ShouldBeTrue();
            var updatedDetails = await updatedDetailsResponse.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            updatedDetails.ShouldNotBeNull();
            updatedDetails.Downloads.ShouldBe(pluginDetails.Downloads + 1);
        }

        [Fact]
        public async Task InstallationFlow_WithMultipleInstallations_ShouldTrackCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var installations = new[]
            {
                new PluginInstallationRequest
                {
                    InstanceId = "instance-1",
                    UserAgent = "User Agent 1",
                    IpAddress = "192.168.1.1"
                },
                new PluginInstallationRequest
                {
                    InstanceId = "instance-2",
                    UserAgent = "User Agent 2",
                    IpAddress = "192.168.1.2"
                },
                new PluginInstallationRequest
                {
                    InstanceId = "instance-3",
                    UserAgent = "User Agent 3",
                    IpAddress = "192.168.1.3"
                }
            };

            // Get initial download count
            var initialResponse = await _client.GetAsync($"/api/plugins/{pluginId}/details");
            initialResponse.IsSuccessStatusCode.ShouldBeTrue();
            var initialDetails = await initialResponse.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            var initialDownloads = initialDetails!.Downloads;

            // Act: Record multiple installations
            var installationIds = new List<string>();
            foreach (var installation in installations)
            {
                var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installation);
                response.IsSuccessStatusCode.ShouldBeTrue();
                var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
                result.ShouldNotBeNull();
                result.Success.ShouldBeTrue();
                installationIds.Add(result.InstallationId!);
            }

            // Assert: Verify all installations were recorded
            installationIds.Count.ShouldBe(3);
            installationIds.All(id => !string.IsNullOrEmpty(id)).ShouldBeTrue();

            // Verify download count increased
            var finalResponse = await _client.GetAsync($"/api/plugins/{pluginId}/details");
            finalResponse.IsSuccessStatusCode.ShouldBeTrue();
            var finalDetails = await finalResponse.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            finalDetails!.Downloads.ShouldBe(initialDownloads + 3);
        }

        [Fact]
        public async Task InstallationFlow_WithConcurrentInstallations_ShouldHandleCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var tasks = new List<Task<PluginInstallationResponse>>();

            // Act: Create multiple concurrent installation requests
            for (int i = 0; i < 5; i++)
            {
                var installation = new PluginInstallationRequest
                {
                    InstanceId = $"concurrent-instance-{i}",
                    UserAgent = $"Concurrent User Agent {i}",
                    IpAddress = $"192.168.1.{i + 10}"
                };

                tasks.Add(Task.Run(async () =>
                {
                    var response = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installation);
                    response.IsSuccessStatusCode.ShouldBeTrue();
                    var result = await response.Content.ReadFromJsonAsync<PluginInstallationResponse>();
                    return result!;
                }));
            }

            // Wait for all installations to complete
            var results = await Task.WhenAll(tasks);

            // Assert: All installations should succeed
            results.All(r => r.Success).ShouldBeTrue();
            results.All(r => !string.IsNullOrEmpty(r.InstallationId)).ShouldBeTrue();
            results.Length.ShouldBe(5);
        }

        [Fact]
        public async Task InstallationFlow_WithInvalidPlugin_ShouldFailGracefully()
        {
            // Arrange
            var invalidPluginId = "non-existent-plugin";
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "test-instance-123",
                UserAgent = "Test User Agent",
                IpAddress = "127.0.0.1"
            };

            // Step 1: Try to get plugin details (should fail)
            var detailsResponse = await _client.GetAsync($"/api/plugins/{invalidPluginId}/details");
            detailsResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);

            // Step 2: Try to get download information (should fail)
            var downloadResponse = await _client.GetAsync($"/api/plugins/{invalidPluginId}/download");
            downloadResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);

            // Step 3: Try to record installation (should fail)
            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{invalidPluginId}/install", installationRequest);
            installResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task InstallationFlow_WithVersionSpecificDownload_ShouldWork()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var version = "1.0.0";

            // Step 1: Get download info for specific version
            var downloadResponse = await _client.GetAsync($"/api/plugins/{pluginId}/download?version={version}");
            downloadResponse.IsSuccessStatusCode.ShouldBeTrue();
            var downloadInfo = await downloadResponse.Content.ReadFromJsonAsync<PluginDownloadInfo>();
            downloadInfo.ShouldNotBeNull();
            downloadInfo.Version.ShouldBe(version);

            // Step 2: Record installation
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "version-test-instance",
                UserAgent = "Version Test User Agent",
                IpAddress = "127.0.0.1"
            };

            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);
            installResponse.IsSuccessStatusCode.ShouldBeTrue();
            var installResult = await installResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            installResult.ShouldNotBeNull();
            installResult.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task InstallationFlow_WithMainApiIntegration_ShouldWork()
        {
            // Arrange
            var pluginId = "test-plugin-1";

            // Step 1: Get plugins via main API
            var mainApiResponse = await _client.GetAsync("/api/plugins/main-api?page=1&pageSize=10");
            mainApiResponse.IsSuccessStatusCode.ShouldBeTrue();
            var pluginsResponse = await mainApiResponse.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            pluginsResponse.ShouldNotBeNull();
            pluginsResponse.Plugins.ShouldNotBeNull();

            // Find our test plugin
            var testPlugin = pluginsResponse.Plugins.FirstOrDefault(p => p.Id == pluginId);
            testPlugin.ShouldNotBeNull();

            // Step 2: Get plugin details
            var detailsResponse = await _client.GetAsync($"/api/plugins/{pluginId}/details");
            detailsResponse.IsSuccessStatusCode.ShouldBeTrue();
            var pluginDetails = await detailsResponse.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            pluginDetails.ShouldNotBeNull();

            // Step 3: Record installation
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "main-api-test-instance",
                UserAgent = "Main API Test User Agent",
                IpAddress = "127.0.0.1"
            };

            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);
            installResponse.IsSuccessStatusCode.ShouldBeTrue();
            var installResult = await installResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            installResult.ShouldNotBeNull();
            installResult.Success.ShouldBeTrue();

            // Step 4: Verify updated download count in main API
            var updatedMainApiResponse = await _client.GetAsync("/api/plugins/main-api?page=1&pageSize=10");
            updatedMainApiResponse.IsSuccessStatusCode.ShouldBeTrue();
            var updatedPluginsResponse = await updatedMainApiResponse.Content.ReadFromJsonAsync<MarketplacePluginsResponse>();
            updatedPluginsResponse.ShouldNotBeNull();
            
            var updatedTestPlugin = updatedPluginsResponse.Plugins.FirstOrDefault(p => p.Id == pluginId);
            updatedTestPlugin.ShouldNotBeNull();
            updatedTestPlugin.Downloads.ShouldBe(testPlugin.Downloads + 1);
        }

        [Fact]
        public async Task InstallationFlow_WithErrorRecovery_ShouldHandleGracefully()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "error-recovery-instance",
                UserAgent = "Error Recovery User Agent",
                IpAddress = "127.0.0.1"
            };

            // Step 1: Record installation
            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);
            installResponse.IsSuccessStatusCode.ShouldBeTrue();
            var installResult = await installResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            installResult.ShouldNotBeNull();
            installResult.Success.ShouldBeTrue();

            // Step 2: Try to install again (should still work)
            var secondInstallResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);
            secondInstallResponse.IsSuccessStatusCode.ShouldBeTrue();
            var secondInstallResult = await secondInstallResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            secondInstallResult.ShouldNotBeNull();
            secondInstallResult.Success.ShouldBeTrue();

            // Step 3: Verify both installations were recorded
            var detailsResponse = await _client.GetAsync($"/api/plugins/{pluginId}/details");
            detailsResponse.IsSuccessStatusCode.ShouldBeTrue();
            var pluginDetails = await detailsResponse.Content.ReadFromJsonAsync<MarketplacePluginDetails>();
            pluginDetails.ShouldNotBeNull();
            pluginDetails.Downloads.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task InstallationFlow_WithLargeData_ShouldHandleCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var largeUserAgent = new string('A', 1000); // Very large user agent
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "large-data-instance-" + new string('B', 100),
                UserAgent = largeUserAgent,
                IpAddress = "127.0.0.1"
            };

            // Act
            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);

            // Assert
            installResponse.IsSuccessStatusCode.ShouldBeTrue();
            var installResult = await installResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            installResult.ShouldNotBeNull();
            installResult.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task InstallationFlow_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var pluginId = "test-plugin-1";
            var installationRequest = new PluginInstallationRequest
            {
                InstanceId = "special-chars-instance!@#$%^&*()",
                UserAgent = "Special Chars User Agent!@#$%^&*()",
                IpAddress = "127.0.0.1"
            };

            // Act
            var installResponse = await _client.PostAsJsonAsync($"/api/plugins/{pluginId}/install", installationRequest);

            // Assert
            installResponse.IsSuccessStatusCode.ShouldBeTrue();
            var installResult = await installResponse.Content.ReadFromJsonAsync<PluginInstallationResponse>();
            installResult.ShouldNotBeNull();
            installResult.Success.ShouldBeTrue();
        }
    }
}
