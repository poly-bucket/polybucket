using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Tests.Factories;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections
{
    public class CollectionPasswordProtectionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CollectionPasswordProtectionTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateCollection_WithUnlistedVisibilityAndPassword_ShouldHashPassword()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

            var userFactory = new TestUserFactory(context);
            var user = await userFactory.CreateTestUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var command = new
            {
                name = "Test Collection",
                description = "Test Description",
                visibility = "Unlisted",
                password = "testpassword123"
            };

            var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

            // Act
            var response = await _client.PostAsync("/api/collections", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            var createdCollection = await context.Collections.FirstOrDefaultAsync(c => c.Name == "Test Collection");
            createdCollection.ShouldNotBeNull();
            createdCollection.OwnerId.ShouldBe(user.Id);
            createdCollection.Visibility.ShouldBe(CollectionVisibility.Unlisted);
            createdCollection.PasswordHash.ShouldNotBeNull();
            createdCollection.PasswordHash.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task AccessCollection_WithCorrectPassword_ShouldReturnCollection()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

            var userFactory = new TestUserFactory(context);
            var user = await userFactory.CreateTestUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "Password Protected Collection",
                Description = "Test Description",
                Visibility = CollectionVisibility.Unlisted,
                OwnerId = user.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword123")
            };
            await context.Collections.AddAsync(collection);
            await context.SaveChangesAsync();

            var accessCommand = new
            {
                collectionId = collection.Id,
                password = "testpassword123"
            };

            var content = new StringContent(JsonSerializer.Serialize(accessCommand), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/api/collections/{collection.Id}/access", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var returnedCollection = JsonSerializer.Deserialize<Collection>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            returnedCollection.ShouldNotBeNull();
            returnedCollection.Id.ShouldBe(collection.Id);
            returnedCollection.Name.ShouldBe("Password Protected Collection");
        }

        [Fact]
        public async Task AccessCollection_WithIncorrectPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

            var userFactory = new TestUserFactory(context);
            var user = await userFactory.CreateTestUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "Password Protected Collection",
                Description = "Test Description",
                Visibility = CollectionVisibility.Unlisted,
                OwnerId = user.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword123")
            };
            await context.Collections.AddAsync(collection);
            await context.SaveChangesAsync();

            var accessCommand = new
            {
                collectionId = collection.Id,
                password = "wrongpassword"
            };

            var content = new StringContent(JsonSerializer.Serialize(accessCommand), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/api/collections/{collection.Id}/access", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AccessCollection_WithoutPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

            var userFactory = new TestUserFactory(context);
            var user = await userFactory.CreateTestUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "Password Protected Collection",
                Description = "Test Description",
                Visibility = CollectionVisibility.Unlisted,
                OwnerId = user.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword123")
            };
            await context.Collections.AddAsync(collection);
            await context.SaveChangesAsync();

            var accessCommand = new
            {
                collectionId = collection.Id
            };

            var content = new StringContent(JsonSerializer.Serialize(accessCommand), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/api/collections/{collection.Id}/access", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        private string CreateJwtToken(string userId)
        {
            // This is a simplified JWT token creation for testing
            // In a real implementation, you would use the actual JWT service
            var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"sub\":\"{userId}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
            var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-signature"));
            
            return $"{header}.{payload}.{signature}";
        }
    }
} 