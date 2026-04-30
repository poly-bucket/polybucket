using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Tests.Factories;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections;

[Collection("TestCollection")]
public class CollectionPasswordProtectionTests : BaseIntegrationTest
{
    public CollectionPasswordProtectionTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact]
    public async Task CreateCollection_WithUnlistedVisibilityAndPassword_ShouldHashPassword()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

        var command = new
        {
            name = "Test Collection",
            description = "Test Description",
            visibility = "Unlisted",
            password = "testpassword123"
        };

        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.PostAsync("/api/collections", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdCollection = await context.Collections.FirstOrDefaultAsync(c => c.Name == "Test Collection");
        createdCollection.ShouldNotBeNull();
        createdCollection!.OwnerId.ShouldBe(user.Id);
        createdCollection.Visibility.ShouldBe(CollectionVisibility.Unlisted);
        createdCollection.PasswordHash.ShouldNotBeNull();
        createdCollection.PasswordHash.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AccessCollection_WithCorrectPassword_ShouldReturnCollection()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

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
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/collections/{collection.Id}/access", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedCollection = JsonSerializer.Deserialize<Collection>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        returnedCollection.ShouldNotBeNull();
        returnedCollection!.Id.ShouldBe(collection.Id);
        returnedCollection.Name.ShouldBe("Password Protected Collection");
    }

    [Fact]
    public async Task AccessCollection_WithIncorrectPassword_ShouldReturnUnauthorized()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

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
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/collections/{collection.Id}/access", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessCollection_WithoutPassword_ShouldReturnUnauthorized()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

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
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/collections/{collection.Id}/access", content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

}
