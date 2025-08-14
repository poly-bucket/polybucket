using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api;
using PolyBucket.Api.Features.Collections.CreateCollection.Http;
using PolyBucket.Api.Features.Collections.DeleteCollection.Http;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using PolyBucket.Api.Features.Collections.UpdateCollection.Http;
using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Http;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Http;
using PolyBucket.Api.Features.Collections.CreateCollection.Domain;
using PolyBucket.Api.Features.Collections.UpdateCollection.Domain;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Domain;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Domain;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Tests.Factories;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections;

public class CollectionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CollectionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(CreateCollectionController) },
            new object[] { typeof(DeleteCollectionController) },
            new object[] { typeof(GetCollectionByIdController) },
            new object[] { typeof(UpdateCollectionController) },
            new object[] { typeof(GetUserCollectionsController) },
            new object[] { typeof(AddModelToCollectionController) },
            new object[] { typeof(RemoveModelFromCollectionController) }
        };
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerAndRoute(Type controllerType)
    {
        // Arrange & Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();

        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
        routeAttr.Template.ShouldBe("api/collections");
    }

    [Fact]
    public async Task CreateCollection_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        
        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var command = new CreateCollectionCommand
        {
            Name = "Test Collection",
            Description = "Test Description",
            Visibility = CollectionVisibility.Private
        };

        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        
        // Mock authentication
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

        // Act
        var response = await _client.PostAsync("/api/collections", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var createdCollection = await context.Collections.FirstOrDefaultAsync(c => c.Name == "Test Collection");
        createdCollection.ShouldNotBeNull();
        createdCollection.OwnerId.ShouldBe(user.Id);
        createdCollection.Visibility.ShouldBe(CollectionVisibility.Private);
    }

    [Fact]
    public async Task GetCollectionById_WithValidId_ShouldReturnCollection()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        await context.Users.AddAsync(user);

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Description = "Test Description",
            Visibility = CollectionVisibility.Public,
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/collections/{collection.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedCollection = JsonSerializer.Deserialize<Collection>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        returnedCollection.ShouldNotBeNull();
        returnedCollection.Id.ShouldBe(collection.Id);
        returnedCollection.Name.ShouldBe("Test Collection");
    }

    [Fact]
    public async Task UpdateCollection_WithValidData_ShouldReturnUpdatedCollection()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        await context.Users.AddAsync(user);

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Description = "Original Description",
            Visibility = CollectionVisibility.Private,
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        var updateCommand = new UpdateCollectionCommand
        {
            Id = collection.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            Visibility = CollectionVisibility.Public
        };

        var content = new StringContent(JsonSerializer.Serialize(updateCommand), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

        // Act
        var response = await _client.PutAsync($"/api/collections/{collection.Id}", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updatedCollection = await context.Collections.FindAsync(collection.Id);
        updatedCollection.ShouldNotBeNull();
        updatedCollection.Name.ShouldBe("Updated Name");
        updatedCollection.Description.ShouldBe("Updated Description");
        updatedCollection.Visibility.ShouldBe(CollectionVisibility.Public);
    }

    [Fact]
    public async Task DeleteCollection_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        await context.Users.AddAsync(user);

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "To Be Deleted",
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

        // Act
        var response = await _client.DeleteAsync($"/api/collections/{collection.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedCollection = await context.Collections.FindAsync(collection.Id);
        deletedCollection.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserCollections_ShouldReturnUserCollections()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        await context.Users.AddAsync(user);

        var collections = new List<Collection>
        {
            new Collection { Name = "Collection 1", OwnerId = user.Id },
            new Collection { Name = "Collection 2", OwnerId = user.Id }
        };
        await context.Collections.AddRangeAsync(collections);
        await context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

        // Act
        var response = await _client.GetAsync("/api/collections/mine");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        result.ShouldNotBeNull();
        result.collections.ShouldNotBeNull();
        result.totalCount.ShouldBe(2);
        result.page.ShouldBe(1);
        result.pageSize.ShouldBe(12);
        result.totalPages.ShouldBe(1);
    }

    [Fact]
    public async Task AddModelToCollection_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        var modelFactory = new TestModelFactory(context);
        var model = await modelFactory.CreateTestModel();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        var command = new AddModelToCollectionCommand
        {
            CollectionId = collection.Id,
            ModelId = model.Id
        };

        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

        // Act
        var response = await _client.PostAsync("/api/collections/add-model", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var collectionModel = await context.CollectionModels.FirstOrDefaultAsync(cm => cm.CollectionId == collection.Id && cm.ModelId == model.Id);
        collectionModel.ShouldNotBeNull();
    }

    [Fact]
    public async Task RemoveModelFromCollection_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        var modelFactory = new TestModelFactory(context);
        var model = await modelFactory.CreateTestModel();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        var collectionModel = new CollectionModel
        {
            CollectionId = collection.Id,
            ModelId = model.Id
        };
        await context.CollectionModels.AddAsync(collectionModel);
        await context.SaveChangesAsync();

        var command = new RemoveModelFromCollectionCommand
        {
            CollectionId = collection.Id,
            ModelId = model.Id
        };

        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateJwtToken(user.Id.ToString())}");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/collections/remove-model")
        {
            Content = content
        };
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var removedCollectionModel = await context.CollectionModels.FirstOrDefaultAsync(cm => cm.CollectionId == collection.Id && cm.ModelId == model.Id);
        removedCollectionModel.ShouldBeNull();
    }

    private string CreateJwtToken(string userId)
    {
        // This is a simplified JWT token creation for testing purposes
        // In a real application, you would use a proper JWT library
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"sub\":\"{userId}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-signature"));
        
        return $"{header}.{payload}.{signature}";
    }
} 