using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RemoveCategoryFromModel;

[Collection("TestCollection")]
public class RemoveCategoryFromModelControllerTests : BaseIntegrationTest
{
    public RemoveCategoryFromModelControllerTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When removing a category as the owner, the remove category controller unlinks the category and keeps the category row.")]
    public async Task RemoveCategoryFromModel_ByOwner_ShouldUnlinkCategory()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(user.Id);
        var category = await CreateCategoryAsync(user.Id, "Toys");
        model.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/categories/{category.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        DbContext.ChangeTracker.Clear();
        var stored = await DbContext.Models.Include(m => m.Categories).SingleAsync(m => m.Id == model.Id);
        stored.Categories.ShouldBeEmpty();
        (await DbContext.Categories.AnyAsync(c => c.Id == category.Id)).ShouldBeTrue();
    }

    [Fact(DisplayName = "When removing a category that is not linked, the remove category controller returns NotFound.")]
    public async Task RemoveCategoryFromModel_WithMissingLink_ShouldReturnNotFound()
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
        var response = await client.DeleteAsync($"/api/models/{model.Id}/categories/{category.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "When removing a category without authentication, the remove category controller returns Unauthorized.")]
    public async Task RemoveCategoryFromModel_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}/categories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "When removing a category as a non-owner, the remove category controller returns Forbidden.")]
    public async Task RemoveCategoryFromModel_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        await ResetStateAsync();
        var owner = await UserFactory.CreateTestUser("owner@test.com");
        var other = await UserFactory.CreateTestUser("other@test.com");
        var model = await ModelFactory.CreateTestModel(owner.Id);
        var category = await CreateCategoryAsync(owner.Id, "Toys");
        model.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, other.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{model.Id}/categories/{category.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When removing a category from a model that does not exist, the remove category controller returns NotFound.")]
    public async Task RemoveCategoryFromModel_WithUnknownModel_ShouldReturnNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var category = await CreateCategoryAsync(user.Id, "Toys");
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, user.Email, "TestPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/models/{Guid.NewGuid()}/categories/{category.Id}");

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
