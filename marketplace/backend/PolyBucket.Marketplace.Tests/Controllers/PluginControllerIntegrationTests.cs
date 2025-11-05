using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using System.Net.Http.Json;
using Xunit;

namespace PolyBucket.Marketplace.Tests.Controllers
{
    public class PluginControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PluginControllerIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task BrowsePlugins_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                Search = "test",
                Page = 1,
                PageSize = 10,
                SortBy = "downloads",
                SortOrder = "desc"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/plugins/browse", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var browseResponse = await response.Content.ReadFromJsonAsync<PluginBrowseResponse>();
            Assert.NotNull(browseResponse);
            Assert.NotNull(browseResponse.Plugins);
            Assert.True(browseResponse.Page >= 1);
            Assert.True(browseResponse.PageSize >= 1);
        }

        [Fact]
        public async Task BrowsePlugins_WithInvalidPage_ReturnsValidResponse()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                Page = 0, // Invalid page
                PageSize = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/plugins/browse", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var browseResponse = await response.Content.ReadFromJsonAsync<PluginBrowseResponse>();
            Assert.NotNull(browseResponse);
            Assert.Equal(1, browseResponse.Page); // Should be corrected to 1
        }

        [Fact]
        public async Task BrowsePlugins_WithInvalidPageSize_ReturnsValidResponse()
        {
            // Arrange
            var request = new PluginBrowseRequest
            {
                Page = 1,
                PageSize = 200 // Invalid page size (too large)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/plugins/browse", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var browseResponse = await response.Content.ReadFromJsonAsync<PluginBrowseResponse>();
            Assert.NotNull(browseResponse);
            Assert.Equal(20, browseResponse.PageSize); // Should be corrected to 20
        }

        [Fact]
        public async Task GetFeaturedPlugins_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/featured?limit=5");

            // Assert
            response.EnsureSuccessStatusCode();
            var plugins = await response.Content.ReadFromJsonAsync<List<PluginSummary>>();
            Assert.NotNull(plugins);
            Assert.True(plugins.Count <= 5);
        }

        [Fact]
        public async Task GetTrendingPlugins_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/trending?limit=10");

            // Assert
            response.EnsureSuccessStatusCode();
            var plugins = await response.Content.ReadFromJsonAsync<List<PluginSummary>>();
            Assert.NotNull(plugins);
            Assert.True(plugins.Count <= 10);
        }

        [Fact]
        public async Task GetPopularTags_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/tags/popular?limit=20");

            // Assert
            response.EnsureSuccessStatusCode();
            var tags = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(tags);
            Assert.True(tags.Count <= 20);
            Assert.All(tags, tag => Assert.False(string.IsNullOrEmpty(tag)));
        }

        [Fact]
        public async Task GetCategories_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/plugins/categories");

            // Assert
            response.EnsureSuccessStatusCode();
            var categories = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(categories);
            Assert.All(categories, category => Assert.False(string.IsNullOrEmpty(category)));
        }
    }
}
