using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api;
using PolyBucket.Api.Features.Collections.GetFavoriteCollections.Http;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Tests.Factories;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections
{
    [Collection("TestCollection")]
    public class GetFavoriteCollectionsTests : BaseIntegrationTest
{
    public GetFavoriteCollectionsTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

        [Fact]
        public void Controller_ShouldHaveApiControllerAndRoute()
        {
            // Arrange & Act
            var apiAttr = typeof(GetFavoriteCollectionsController).GetCustomAttribute<Microsoft.AspNetCore.Mvc.ApiControllerAttribute>();
            var routeAttr = typeof(GetFavoriteCollectionsController).GetCustomAttribute<Microsoft.AspNetCore.Mvc.RouteAttribute>();

            // Assert
            apiAttr.ShouldNotBeNull();
            routeAttr.ShouldNotBeNull();
            routeAttr.Template.ShouldBe("api/collections");
        }

        [Fact]
        public async Task GetFavoriteCollections_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetStateAsync();
            
            // Act
            var response = await Client.GetAsync("/api/collections/favorites");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetFavoriteCollections_WithValidUser_ShouldReturnFavoriteCollections()
        {
            // Arrange
            await ResetStateAsync();
            
            var user = await CreateTestUser();
            
            // Debug: Check if user was created
            var createdUser = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            Assert.NotNull(createdUser);
            Assert.Equal(user.Email, createdUser.Email);

            // Create test collections with different favorite and display order settings
            var collections = new List<Collection>
            {
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "First Collection",
                    Description = "First description",
                    Visibility = CollectionVisibility.Private,
                    OwnerId = user.Id,
                    Favorite = true,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Second Collection",
                    Description = "Second description",
                    Visibility = CollectionVisibility.Public,
                    OwnerId = user.Id,
                    Favorite = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Third Collection",
                    Description = "Third description",
                    Visibility = CollectionVisibility.Unlisted,
                    OwnerId = user.Id,
                    Favorite = false, // Not favorite
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Fourth Collection",
                    Description = "Fourth description",
                    Visibility = CollectionVisibility.Private,
                    OwnerId = user.Id,
                    Favorite = true,
                    DisplayOrder = 0, // Same display order as first
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await DbContext.Collections.AddRangeAsync(collections);
            await DbContext.SaveChangesAsync();

            // Get authentication token
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            SetAuthHeaders(token);
            
            // Debug: Check the token and headers
            Assert.NotEmpty(token);
            Assert.NotEqual("test-token", token);
            
            // Debug: Check if headers are set
            Assert.True(Client.DefaultRequestHeaders.Contains("Authorization"));
            var authHeader = Client.DefaultRequestHeaders.GetValues("Authorization").FirstOrDefault();
            Assert.NotNull(authHeader);
            Assert.StartsWith("Bearer ", authHeader);

            // Act
            var response = await Client.GetAsync("/api/collections/favorites");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            result.TryGetProperty("collections", out var collectionsElement).ShouldBeTrue();
            var collectionsArray = collectionsElement.EnumerateArray().ToList();
            
            // Should only return 3 favorite collections
            collectionsArray.Count.ShouldBe(3);
            
            // Ordered by DisplayOrder, then Name (alphabetically among ties)
            collectionsArray[0].GetProperty("name").GetString().ShouldBe("First Collection");
            collectionsArray[1].GetProperty("name").GetString().ShouldBe("Fourth Collection");
            collectionsArray[2].GetProperty("name").GetString().ShouldBe("Second Collection");
            
            // Verify properties are correctly mapped
            var firstCollection = collectionsArray[0];
            firstCollection.GetProperty("id").GetString().ShouldNotBeNullOrEmpty();
            firstCollection.GetProperty("name").GetString().ShouldBe("First Collection");
            firstCollection.GetProperty("description").GetString().ShouldBe("First description");
            firstCollection.GetProperty("visibility").GetString().ShouldBe("Private");
            firstCollection.GetProperty("favorite").GetBoolean().ShouldBeTrue();
            firstCollection.GetProperty("displayOrder").GetInt32().ShouldBe(0);
            firstCollection.GetProperty("modelCount").GetInt32().ShouldBe(0);
            firstCollection.TryGetProperty("createdAt", out _).ShouldBeTrue();
            firstCollection.TryGetProperty("updatedAt", out _).ShouldBeTrue();
        }

        [Fact]
        public async Task GetFavoriteCollections_WithNoFavoriteCollections_ShouldReturnEmptyArray()
        {
            // Arrange
            await ResetStateAsync();
            
            var user = await CreateTestUser();

            // Create a non-favorite collection
            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "Non-Favorite Collection",
                Description = "Not a favorite",
                Visibility = CollectionVisibility.Private,
                OwnerId = user.Id,
                Favorite = false,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await DbContext.Collections.AddAsync(collection);
            await DbContext.SaveChangesAsync();

            // Get authentication token
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            SetAuthHeaders(token);

            // Act
            var response = await Client.GetAsync("/api/collections/favorites");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            result.TryGetProperty("collections", out var collectionsElement).ShouldBeTrue();
            var collectionsArray = collectionsElement.EnumerateArray().ToList();
            
            collectionsArray.Count.ShouldBe(0);
        }

        [Fact]
        public async Task GetFavoriteCollections_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetStateAsync();
            
            Client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

            // Act
            var response = await Client.GetAsync("/api/collections/favorites");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }


    }
}
