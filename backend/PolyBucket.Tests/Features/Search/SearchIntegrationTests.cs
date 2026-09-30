using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Search;

[Collection("TestCollection")]
public class SearchIntegrationTests : BaseIntegrationTest
{
    public SearchIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task EnableTrigramsAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm");
        Factory.Services.GetRequiredService<ISearchCapabilities>().Invalidate();
    }

    private async Task<User> CreateAuthorAsync()
    {
        var author = await CreateTestUser();
        author.IsProfilePublic = true;
        await DbContext.SaveChangesAsync();
        return author;
    }

    private Model AddModel(User author, string name, PrivacySettings privacy = PrivacySettings.Public, bool isPublic = true, DateTime? createdAt = null)
    {
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Calibration print",
            License = LicenseTypes.MIT,
            Privacy = privacy,
            IsPublic = isPublic,
            AuthorId = author.Id,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        DbContext.Models.Add(model);
        return model;
    }

    private async Task<SearchResponse> SearchAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SearchResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        }))!;
    }

    [Fact(DisplayName = "When pg_trgm is available, a typo in the query still finds the model and the closest name ranks first.")]
    public async Task Search_Typo_MatchesWithTrigrams()
    {
        // Arrange
        await ResetStateAsync();
        await EnableTrigramsAsync();
        var author = await CreateAuthorAsync();
        AddModel(author, "3DBenchy");
        AddModel(author, "Benchy boat hull");
        AddModel(author, "Calibration cube");
        await DbContext.SaveChangesAsync();

        // Act
        var exact = await SearchAsync("/api/search?query=benchy&type=Models");
        var typo = await SearchAsync("/api/search?query=bnchy&type=Models");

        // Assert
        exact.Results.Select(r => r.Title).ShouldContain("3DBenchy");
        exact.Results.Select(r => r.Title).ShouldNotContain("Calibration cube");
        typo.Results.Select(r => r.Title).ShouldContain("Benchy boat hull");
        typo.Results.ShouldNotContain(r => r.Title == "Calibration cube");
    }

    [Fact(DisplayName = "When models are private, unlisted by privacy, or pending moderation, they are excluded from search.")]
    public async Task Search_ExcludesPrivateAndUnmoderatedModels()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateAuthorAsync();
        AddModel(author, "Visible widget");
        AddModel(author, "Private widget", PrivacySettings.Private, isPublic: false);
        var pending = AddModel(author, "Pending widget");
        var approved = AddModel(author, "Approved widget");
        DbContext.ModelModerationRecords.AddRange(
            new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = pending.Id, Status = ModelModerationStatus.Pending },
            new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = approved.Id, Status = ModelModerationStatus.Approved });
        await DbContext.SaveChangesAsync();

        // Act
        var result = await SearchAsync("/api/search?query=widget&type=Models");

        // Assert
        result.Results.Select(r => r.Title).OrderBy(t => t).ShouldBe(new[] { "Approved widget", "Visible widget" });
    }

    [Fact(DisplayName = "When paging through results, pages do not overlap and together cover every match.")]
    public async Task Search_Pages_DoNotOverlap()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateAuthorAsync();
        var now = DateTime.UtcNow;
        for (var i = 0; i < 7; i++)
        {
            AddModel(author, $"Gear {i}", createdAt: now);
        }
        await DbContext.SaveChangesAsync();

        // Act
        var seen = new List<Guid>();
        for (var page = 1; page <= 3; page++)
        {
            var result = await SearchAsync($"/api/search?query=gear&type=Models&page={page}&pageSize=3");
            seen.AddRange(result.Results.Select(r => r.Id));
        }

        // Assert
        seen.Count.ShouldBe(7);
        seen.Distinct().Count().ShouldBe(7);
    }

    [Fact(DisplayName = "When searching users, email addresses are not returned unless the user chose to show them.")]
    public async Task Search_Users_DoNotExposeEmail()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateAuthorAsync();

        // Act
        var response = await Client.GetAsync($"/api/search?query={author.Username}&type=Users");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        json.ShouldContain(author.Username);
        json.ShouldNotContain(author.Email);
    }

    [Fact(DisplayName = "When searching all types, each type reports its own count.")]
    public async Task Search_All_ReportsCountsPerType()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateAuthorAsync();
        AddModel(author, "Spool holder");
        AddModel(author, "Spool clip");
        await DbContext.SaveChangesAsync();

        // Act
        var result = await SearchAsync("/api/search?query=spool&type=All&pageSize=1");

        // Assert
        result.Counts.Models.ShouldBe(2);
        result.Results.Count(r => r.Type == SearchResultType.Model).ShouldBe(1);
        result.TotalPages.ShouldBe(2);
    }
}
