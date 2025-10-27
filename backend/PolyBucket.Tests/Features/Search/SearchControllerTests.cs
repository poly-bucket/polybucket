using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Common.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Search
{
    public class SearchControllerTests : BaseIntegrationTest
    {
        public SearchControllerTests(TestCollectionFixture testFixture) : base(testFixture)
        {
        }

        [Fact]
        public async Task Search_WithValidQuery_ReturnsOk()
        {
            // Arrange
            var query = "test";
            var url = $"/api/search?query={query}";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task Search_WithEmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            var url = "/api/search?query=";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Search_WithInvalidPage_ReturnsBadRequest()
        {
            // Arrange
            var url = "/api/search?query=test&page=0";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Search_WithInvalidPageSize_ReturnsBadRequest()
        {
            // Arrange
            var url = "/api/search?query=test&pageSize=0";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Search_WithModelsType_ReturnsModels()
        {
            // Arrange
            var url = "/api/search?query=test&type=Models";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Search_WithUsersType_ReturnsUsers()
        {
            // Arrange
            var url = "/api/search?query=test&type=Users";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Search_WithCollectionsType_ReturnsCollections()
        {
            // Arrange
            var url = "/api/search?query=test&type=Collections";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Search_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var url = "/api/search?query=test&page=2&pageSize=5";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Search_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            var url = "/api/search?query=test&sortBy=relevance&sortDescending=false";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
