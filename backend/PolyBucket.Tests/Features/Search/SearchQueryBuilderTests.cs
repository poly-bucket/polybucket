using System;
using System.Collections.Generic;
using System.Linq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Search;

public class SearchQueryBuilderTests
{
    private static readonly User Author = new() { Id = Guid.NewGuid(), Username = "maker", Email = "maker@example.com" };

    private static Model CreateModel(string name, string? description = null, DateTime? createdAt = null, int downloads = 0) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = description,
        Author = Author,
        AuthorId = Author.Id,
        CreatedAt = createdAt ?? DateTime.UtcNow,
        Downloads = downloads
    };

    private static List<string> RankNames(IEnumerable<Model> models, string term, string? sortBy = null, bool descending = false)
    {
        var scored = SearchQueryBuilder.ScoreModels(models.AsQueryable(), SearchQueryBuilder.NormalizeTerm(term), SearchTextMode.Basic);
        return SearchQueryBuilder.OrderModels(scored, sortBy, descending).Select(s => s.Entity.Name).ToList();
    }

    [Fact(DisplayName = "When ranking by relevance, exact name matches come before prefix, then substring, then description-only matches.")]
    public void OrderModels_Relevance_RanksExactThenPrefixThenContainsThenDescription()
    {
        // Arrange
        var models = new[]
        {
            CreateModel("Printable gear", description: "benchy compatible"),
            CreateModel("Classic benchy"),
            CreateModel("Benchy remix"),
            CreateModel("Benchy")
        };

        // Act
        var ranked = RankNames(models, "  BENCHY ");

        // Assert
        ranked.ShouldBe(new[] { "Benchy", "Benchy remix", "Classic benchy", "Printable gear" });
    }

    [Fact(DisplayName = "When scores tie, newer items come first and ids break any remaining ties so pages never overlap.")]
    public void OrderModels_Ties_AreBrokenByCreatedAtThenId()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var models = new[]
        {
            CreateModel("Benchy older", createdAt: now.AddDays(-2)),
            CreateModel("Benchy newer", createdAt: now)
        };

        // Act
        var ranked = RankNames(models, "benchy");

        // Assert
        ranked.ShouldBe(new[] { "Benchy newer", "Benchy older" });
    }

    [Fact(DisplayName = "When sorting by downloads descending, relevance is ignored.")]
    public void OrderModels_ByDownloads_IgnoresRelevance()
    {
        // Arrange
        var models = new[]
        {
            CreateModel("Benchy", downloads: 1),
            CreateModel("Benchy remix", downloads: 50)
        };

        // Act
        var ranked = RankNames(models, "benchy", "downloads", descending: true);

        // Assert
        ranked.ShouldBe(new[] { "Benchy remix", "Benchy" });
    }

    [Fact(DisplayName = "When nothing in the name, description, tags, categories, or author matches, the model is filtered out.")]
    public void ScoreModels_NoMatch_IsFiltered()
    {
        // Arrange
        var models = new[] { CreateModel("Calibration cube") };

        // Act
        var ranked = RankNames(models, "benchy");

        // Assert
        ranked.ShouldBeEmpty();
    }

    [Theory(DisplayName = "When building LIKE patterns, wildcard characters in user input are escaped.")]
    [InlineData("100%", "%100\\%%")]
    [InlineData("a_b", "%a\\_b%")]
    [InlineData("c:\\x", "%c:\\\\x%")]
    public void ContainsPattern_EscapesWildcards(string term, string expected)
    {
        // Arrange
        var input = term;

        // Act
        var pattern = SearchQueryBuilder.ContainsPattern(input);

        // Assert
        pattern.ShouldBe(expected);
    }
}
