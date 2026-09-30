using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments;

[Collection("TestCollection")]
public class CommentReactionsIntegrationTests : BaseIntegrationTest
{
    public CommentReactionsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task<(EnhancedComment Comment, User Author)> SeedCommentAsync()
    {
        var author = await CreateTestUser();
        var model = await ModelFactory.CreateTestModel("Reaction model", "desc", author.Id);
        var comment = new EnhancedComment
        {
            Id = Guid.NewGuid(),
            Content = "Great print",
            AuthorId = author.Id,
            TargetId = model.Id,
            TargetType = CommentTargetType.Model,
            CreatedAt = DateTime.UtcNow,
            CreatedById = author.Id,
            UpdatedById = author.Id
        };
        DbContext.EnhancedComments.Add(comment);
        await DbContext.SaveChangesAsync();
        return (comment, author);
    }

    private async Task<CommentReactionOutcome> ReactInOwnScopeAsync(Guid commentId, Guid userId, CommentReactionType type)
    {
        using var scope = Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ICommentReactionService>().ReactAsync(commentId, userId, type);
    }

    private async Task<(int Likes, int Dislikes)> ReadCountsAsync(Guid commentId)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        var comment = await context.EnhancedComments.AsNoTracking().SingleAsync(c => c.Id == commentId);
        return (comment.Likes, comment.Dislikes);
    }

    [Fact(DisplayName = "When two users like the same comment at the same time, both likes are counted.")]
    public async Task ConcurrentLikes_FromTwoUsers_CountBoth()
    {
        // Arrange
        await ResetStateAsync();
        var (comment, _) = await SeedCommentAsync();
        var first = await CreateTestUser();
        var second = await CreateTestUser();

        // Act
        await Task.WhenAll(
            ReactInOwnScopeAsync(comment.Id, first.Id, CommentReactionType.Like),
            ReactInOwnScopeAsync(comment.Id, second.Id, CommentReactionType.Like));

        // Assert
        var (likes, dislikes) = await ReadCountsAsync(comment.Id);
        likes.ShouldBe(2);
        dislikes.ShouldBe(0);
    }

    [Fact(DisplayName = "When the same user likes a comment concurrently, only one like is stored and counted.")]
    public async Task ConcurrentLikes_FromSameUser_CountOnce()
    {
        // Arrange
        await ResetStateAsync();
        var (comment, _) = await SeedCommentAsync();
        var liker = await CreateTestUser();

        // Act
        await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => ReactInOwnScopeAsync(comment.Id, liker.Id, CommentReactionType.Like)));

        // Assert
        var (likes, _) = await ReadCountsAsync(comment.Id);
        likes.ShouldBe(1);
        (await DbContext.CommentReactions.AsNoTracking().CountAsync(r => r.CommentId == comment.Id)).ShouldBe(1);
    }

    [Fact(DisplayName = "When a duplicate reaction row is inserted directly, the unique index rejects it.")]
    public async Task DuplicateReaction_IsRejectedByUniqueIndex()
    {
        // Arrange
        await ResetStateAsync();
        var (comment, author) = await SeedCommentAsync();
        DbContext.CommentReactions.Add(new CommentReaction { Id = Guid.NewGuid(), CommentId = comment.Id, UserId = author.Id, Type = CommentReactionType.Like, CreatedAt = DateTime.UtcNow });
        await DbContext.SaveChangesAsync();
        DbContext.CommentReactions.Add(new CommentReaction { Id = Guid.NewGuid(), CommentId = comment.Id, UserId = author.Id, Type = CommentReactionType.Dislike, CreatedAt = DateTime.UtcNow });

        // Act
        var act = () => DbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact(DisplayName = "When a user switches from like to dislike and then removes it, the counters follow each step.")]
    public async Task LikeDislikeRemove_UpdatesCounters()
    {
        // Arrange
        await ResetStateAsync();
        var (comment, _) = await SeedCommentAsync();
        var user = await CreateTestUser();

        // Act
        await ReactInOwnScopeAsync(comment.Id, user.Id, CommentReactionType.Like);
        var afterLike = await ReadCountsAsync(comment.Id);
        await ReactInOwnScopeAsync(comment.Id, user.Id, CommentReactionType.Dislike);
        var afterSwitch = await ReadCountsAsync(comment.Id);
        using (var scope = Factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ICommentReactionService>().RemoveReactionAsync(comment.Id, user.Id, CommentReactionType.Dislike);
        }
        var afterRemove = await ReadCountsAsync(comment.Id);

        // Assert
        afterLike.ShouldBe((1, 0));
        afterSwitch.ShouldBe((0, 1));
        afterRemove.ShouldBe((0, 0));
    }

    [Fact(DisplayName = "When a user comments and likes through the API, the listing shows the comment with the user's like.")]
    public async Task Api_CreateAndLike_ShowsUserReaction()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser("commenter@polybucket.test", "Password123!");
        var model = await ModelFactory.CreateTestModel("Commented model", "desc", user.Id);
        var token = await GetAuthToken(user.Email, "Password123!");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var create = await Client.PostAsJsonAsync("/api/comments", new
        {
            target = new { targetId = model.Id, targetType = "Model" },
            content = "  First!  "
        });
        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var commentId = created.RootElement.GetProperty("id").GetGuid();
        var like = await Client.PostAsync($"/api/comments/{commentId}/like", null);
        var list = await Client.GetAsync($"/api/comments/target/model/{model.Id}");
        using var listed = JsonDocument.Parse(await list.Content.ReadAsStringAsync());

        // Assert
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        like.StatusCode.ShouldBe(HttpStatusCode.OK);
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        var first = listed.RootElement.GetProperty("comments").EnumerateArray().Single();
        first.GetProperty("content").GetString().ShouldBe("First!");
        first.GetProperty("likes").GetInt32().ShouldBe(1);
        first.GetProperty("userHasLiked").GetBoolean().ShouldBeTrue();
        first.GetProperty("canEdit").GetBoolean().ShouldBeTrue();
    }
}
