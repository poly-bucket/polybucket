using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.AddCategoryToModel;

[Collection("TestCollection")]
public class AddCategoryToModelControllerTests : BaseIntegrationTest
{
    public AddCategoryToModelControllerTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When adding a category as the owner, the add category controller links the category once.")]
    public async Task AddCategoryToModel_ByOwner_ShouldLinkCategoryOnce()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var category = await CreateCategoryAsync(user.Id, "Toys");
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var first = await client.PostAsync($"/api/models/{model.Id}/categories/{category.Id}", null);
        var second = await client.PostAsync($"/api/models/{model.Id}/categories/{category.Id}", null);

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        DbContext.ChangeTracker.Clear();
        var stored = await DbContext.Models.Include(m => m.Categories).SingleAsync(m => m.Id == model.Id);
        stored.Categories.ShouldHaveSingleItem().Id.ShouldBe(category.Id);
    }

    [Fact(DisplayName = "When adding a category that does not exist, the add category controller returns NotFound.")]
    public async Task AddCategoryToModel_WithUnknownCategory_ShouldReturnNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/categories/{Guid.NewGuid()}", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When adding a category without authentication, the add category controller returns Unauthorized.")]
    public async Task AddCategoryToModel_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/categories/{Guid.NewGuid()}", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When adding a category as a non-owner, the add category controller returns Forbidden.")]
    public async Task AddCategoryToModel_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        await ResetStateAsync();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var other = await UserFactory.CreateTestUser("other@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);
        var category = await CreateCategoryAsync(owner.Id, "Toys");
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, other.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync($"/api/models/{model.Id}/categories/{category.Id}", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When adding a category to a model that does not exist, the add category controller returns NotFound.")]
    public async Task AddCategoryToModel_WithUnknownModel_ShouldReturnNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var category = await CreateCategoryAsync(user.Id, "Toys");
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync($"/api/models/{Guid.NewGuid()}/categories/{category.Id}", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<Category> CreateCategoryAsync(Guid userId, string name)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = string.Empty,
            Icon = string.Empty,
            Color = string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            UpdatedById = userId
        };
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        return category;
    }
}
