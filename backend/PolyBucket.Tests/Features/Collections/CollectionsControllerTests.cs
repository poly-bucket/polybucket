using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Domain;
using PolyBucket.Api.Features.Collections.CreateCollection.Domain;
using PolyBucket.Api.Features.Collections.CreateCollection.Http;
using PolyBucket.Api.Features.Collections.DeleteCollection.Http;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Domain;
using PolyBucket.Api.Features.Collections.UpdateCollection.Domain;
using PolyBucket.Api.Features.Collections.UpdateCollection.Http;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Http;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Http;
using PolyBucket.Tests.Factories;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections;

[Collection("TestCollection")]
public class CollectionsControllerTests : BaseIntegrationTest
{
    public CollectionsControllerTests(TestCollectionFixture testFixture) : base(testFixture)
    {
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
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();

        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
        routeAttr!.Template.ShouldBe("api/collections");
    }

    [Fact]
    public async Task CreateCollection_WithValidData_ShouldReturnOk()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

        var command = new CreateCollectionCommand
        {
            Name = "Test Collection",
            Description = "Test Description",
            Visibility = CollectionVisibility.Private
        };

        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.PostAsync("/api/collections", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var createdCollection = await context.Collections.FirstOrDefaultAsync(c => c.Name == "Test Collection");
        createdCollection.ShouldNotBeNull();
        createdCollection!.OwnerId.ShouldBe(user.Id);
        createdCollection.Visibility.ShouldBe(CollectionVisibility.Private);
    }

    [Fact]
    public async Task GetCollectionById_WithValidId_ShouldReturnCollection()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

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

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/api/collections/{collection.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedCollection = JsonSerializer.Deserialize<Collection>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        returnedCollection.ShouldNotBeNull();
        returnedCollection!.Id.ShouldBe(collection.Id);
        returnedCollection.Name.ShouldBe("Test Collection");
    }

    [Fact]
    public async Task UpdateCollection_WithValidData_ShouldReturnUpdatedCollection()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

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
            Visibility = CollectionVisibility.Public,
            Avatar = "data:image/png;base64,dGVzdA=="
        };

        var content = new StringContent(JsonSerializer.Serialize(updateCommand), Encoding.UTF8, "application/json");
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.PutAsync($"/api/collections/{collection.Id}", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        context.ChangeTracker.Clear();
        var updatedCollection = await context.Collections.FindAsync(collection.Id);
        updatedCollection.ShouldNotBeNull();
        updatedCollection!.Name.ShouldBe("Updated Name");
        updatedCollection.Description.ShouldBe("Updated Description");
        updatedCollection.Visibility.ShouldBe(CollectionVisibility.Public);
        updatedCollection.Avatar.ShouldBe("data:image/png;base64,dGVzdA==");
    }

    [Fact]
    public async Task DeleteCollection_WithValidId_ShouldReturnNoContent()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "To Be Deleted",
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.DeleteAsync($"/api/collections/{collection.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        context.ChangeTracker.Clear();
        var deletedCollection = await context.Collections.FindAsync(collection.Id);
        deletedCollection.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserCollections_ShouldReturnUserCollections()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();

        var collections = new List<Collection>
        {
            new Collection { Id = Guid.NewGuid(), Name = "Collection 1", OwnerId = user.Id },
            new Collection { Id = Guid.NewGuid(), Name = "Collection 2", OwnerId = user.Id }
        };
        await context.Collections.AddRangeAsync(collections);
        await context.SaveChangesAsync();

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.GetAsync("/api/collections/mine");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

        result.TryGetProperty("collections", out var collectionsEl).ShouldBeTrue();
        collectionsEl.GetArrayLength().ShouldBe(2);
        result.GetProperty("totalCount").GetInt32().ShouldBe(2);
        result.GetProperty("page").GetInt32().ShouldBe(1);
        result.GetProperty("pageSize").GetInt32().ShouldBe(12);
        result.GetProperty("totalPages").GetInt32().ShouldBe(1);
    }

    [Fact]
    public async Task AddModelToCollection_WithValidData_ShouldReturnNoContent()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        var modelFactory = new TestModelFactory(context);
        var model = await modelFactory.CreateTestModel(user.Id);

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            OwnerId = user.Id
        };
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.PostAsync($"/api/collections/{collection.Id}/models/{model.Id}", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var collectionModel = await context.CollectionModels.FirstOrDefaultAsync(cm => cm.CollectionId == collection.Id && cm.ModelId == model.Id);
        collectionModel.ShouldNotBeNull();
    }

    [Fact]
    public async Task RemoveModelFromCollection_WithValidData_ShouldReturnNoContent()
    {
        await ResetStateAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        var userFactory = new TestUserFactory(context);
        var user = await userFactory.CreateTestUser();
        var modelFactory = new TestModelFactory(context);
        var model = await modelFactory.CreateTestModel(user.Id);

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

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestJwtTokens.Create(user.Id.ToString())}");

        var response = await client.DeleteAsync($"/api/collections/{collection.Id}/models/{model.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var removedCollectionModel = await context.CollectionModels.FirstOrDefaultAsync(cm => cm.CollectionId == collection.Id && cm.ModelId == model.Id);
        removedCollectionModel.ShouldBeNull();
    }

}
