using System;
using System.Linq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.GetModels.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModels;

public class GetModelsRepositoryTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static readonly Model[] Models =
    {
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Beta", CreatedAt = Now, Downloads = 5, Likes = 1 },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Alpha", CreatedAt = Now, Downloads = 5, Likes = 9 },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "Gamma", CreatedAt = Now.AddDays(-1), Downloads = 50, Likes = 3 }
    };

    [Fact(DisplayName = "When no sort is given, models are ordered newest first with id as a stable tie-breaker.")]
    public void ApplyOrdering_Default_IsNewestFirstThenId()
    {
        // Arrange
        var query = Models.AsQueryable();

        // Act
        var names = GetModelsRepository.ApplyOrdering(query, null).Select(m => m.Name).ToList();

        // Assert
        names.ShouldBe(new[] { "Alpha", "Beta", "Gamma" });
    }

    [Theory(DisplayName = "When a known sort is requested, models are ordered by that field.")]
    [InlineData("downloads", new[] { "Gamma", "Alpha", "Beta" })]
    [InlineData("likes", new[] { "Alpha", "Gamma", "Beta" })]
    [InlineData("name", new[] { "Alpha", "Beta", "Gamma" })]
    [InlineData("createdAt", new[] { "Alpha", "Beta", "Gamma" })]
    public void ApplyOrdering_KnownSort_OrdersByField(string sortBy, string[] expected)
    {
        // Arrange
        var query = Models.AsQueryable();

        // Act
        var names = GetModelsRepository.ApplyOrdering(query, sortBy).Select(m => m.Name).ToList();

        // Assert
        names.ShouldBe(expected);
    }

    [Fact(DisplayName = "When paging through the default order, consecutive pages never repeat a model.")]
    public void ApplyOrdering_Paging_HasNoOverlap()
    {
        // Arrange
        var query = Models.AsQueryable();

        // Act
        var first = GetModelsRepository.ApplyOrdering(query, null).Take(2).Select(m => m.Id).ToList();
        var second = GetModelsRepository.ApplyOrdering(query, null).Skip(2).Take(2).Select(m => m.Id).ToList();

        // Assert
        first.Intersect(second).ShouldBeEmpty();
        first.Concat(second).Count().ShouldBe(3);
    }
}
