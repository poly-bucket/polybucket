using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using Shouldly;
using Xunit;

namespace PolyBucket.Database.Tests;

[Collection("DatabaseTestCollection")]
public class LikeConcurrencyIntegrationTests : DatabaseIntegrationTestBase
{
    [Fact(DisplayName = "When two scopes insert the same active like concurrently, only one row remains.")]
    public async Task ConcurrentActiveLikes_OnlyOneSurvives()
    {
        // Arrange
        await ResetDatabaseAsync();
        var user = await SeedUserAsync();
        var model = await SeedModelAsync(user.Id);

        async Task TryInsertLikeAsync()
        {
            await using var ctx = DatabaseTestManager.CreateContext();
            ctx.Likes.Add(new Like
            {
                Id = Guid.NewGuid(),
                ModelId = model.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        // Act
        var tasks = Enumerable.Range(0, 5).Select(_ => TryInsertLikeAsync()).ToArray();
        var exceptions = 0;
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (DbUpdateException)
            {
                exceptions++;
            }
        }

        // Assert
        var count = await DbContext.Likes.CountAsync(l =>
            l.ModelId == model.Id && l.UserId == user.Id && l.DeletedAt == null);
        count.ShouldBe(1);
        exceptions.ShouldBeGreaterThan(0);
    }
}
