using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Data;

[Collection("TestCollection")]
public class LikeConcurrencyIntegrationTests : BaseIntegrationTest
{
    public LikeConcurrencyIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When two scopes insert the same active like concurrently, only one row remains.")]
    public async Task ConcurrentActiveLikes_OnlyOneSurvives()
    {
        // Arrange
        await ResetStateAsync();
        var user = await UserFactory.CreateTestUser();
        var model = await ModelFactory.CreateTestModel(userId: user.Id);

        async Task TryInsertLikeAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
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
