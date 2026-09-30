using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments;

public class CommentReactionServiceTests
{
    private readonly Mock<ICommentReactionRepository> _repository = new();
    private readonly Guid _commentId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public CommentReactionServiceTests()
    {
        _repository.Setup(r => r.IsReactableAsync(_commentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetCountsAsync(_commentId, It.IsAny<CancellationToken>())).ReturnsAsync((3, 1));
    }

    private CommentReactionService CreateService() => new(_repository.Object);

    private void SetExistingReaction(params CommentReactionType?[] sequence)
    {
        var setup = _repository.SetupSequence(r => r.GetUserReactionAsync(_commentId, _userId, It.IsAny<CancellationToken>()));
        foreach (var value in sequence)
        {
            setup = setup.ReturnsAsync(value);
        }
    }

    [Fact(DisplayName = "When a user likes a comment for the first time, a like is added and reported as applied.")]
    public async Task React_FirstLike_AddsReaction()
    {
        // Arrange
        SetExistingReaction(null, CommentReactionType.Like);
        _repository.Setup(r => r.TryAddAsync(_commentId, _userId, CommentReactionType.Like, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var outcome = await CreateService().ReactAsync(_commentId, _userId, CommentReactionType.Like);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.Applied);
        outcome.UserReaction.ShouldBe(CommentReactionType.Like);
        outcome.Likes.ShouldBe(3);
        _repository.Verify(r => r.TrySwitchAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommentReactionType>(), It.IsAny<CommentReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a user likes a comment they already like, nothing is written and the result is unchanged.")]
    public async Task React_RepeatedLike_IsIdempotent()
    {
        // Arrange
        SetExistingReaction(CommentReactionType.Like, CommentReactionType.Like);

        // Act
        var outcome = await CreateService().ReactAsync(_commentId, _userId, CommentReactionType.Like);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.Unchanged);
        outcome.UserReaction.ShouldBe(CommentReactionType.Like);
        _repository.Verify(r => r.TryAddAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommentReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.TrySwitchAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommentReactionType>(), It.IsAny<CommentReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a user dislikes a comment they liked, the reaction is switched rather than duplicated.")]
    public async Task React_LikeThenDislike_SwitchesReaction()
    {
        // Arrange
        SetExistingReaction(CommentReactionType.Like, CommentReactionType.Dislike);
        _repository.Setup(r => r.TrySwitchAsync(_commentId, _userId, CommentReactionType.Like, CommentReactionType.Dislike, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var outcome = await CreateService().ReactAsync(_commentId, _userId, CommentReactionType.Dislike);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.Applied);
        outcome.UserReaction.ShouldBe(CommentReactionType.Dislike);
        _repository.Verify(r => r.TryAddAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommentReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a concurrent request already inserted the like, the unique index rejection is reported as unchanged.")]
    public async Task React_ConcurrentDuplicate_IsUnchanged()
    {
        // Arrange
        SetExistingReaction(null, CommentReactionType.Like);
        _repository.Setup(r => r.TryAddAsync(_commentId, _userId, CommentReactionType.Like, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var outcome = await CreateService().ReactAsync(_commentId, _userId, CommentReactionType.Like);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.Unchanged);
        outcome.UserReaction.ShouldBe(CommentReactionType.Like);
    }

    [Fact(DisplayName = "When a user removes their like, the like is deleted.")]
    public async Task RemoveReaction_Unlike_RemovesLike()
    {
        // Arrange
        SetExistingReaction(CommentReactionType.Like, null);
        _repository.Setup(r => r.TryRemoveAsync(_commentId, _userId, CommentReactionType.Like, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var outcome = await CreateService().RemoveReactionAsync(_commentId, _userId, CommentReactionType.Like);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.Applied);
        outcome.UserReaction.ShouldBeNull();
    }

    [Fact(DisplayName = "When a user removes a like they never gave, their dislike is left alone.")]
    public async Task RemoveReaction_WrongType_IsUnchanged()
    {
        // Arrange
        SetExistingReaction(CommentReactionType.Dislike, CommentReactionType.Dislike);

        // Act
        var outcome = await CreateService().RemoveReactionAsync(_commentId, _userId, CommentReactionType.Like);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.Unchanged);
        outcome.UserReaction.ShouldBe(CommentReactionType.Dislike);
        _repository.Verify(r => r.TryRemoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommentReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the comment is missing or hidden, reacting returns not found without writing.")]
    public async Task React_HiddenComment_ReturnsNotFound()
    {
        // Arrange
        _repository.Setup(r => r.IsReactableAsync(_commentId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var outcome = await CreateService().ReactAsync(_commentId, _userId, CommentReactionType.Like);

        // Assert
        outcome.Change.ShouldBe(CommentReactionChange.NotFound);
        _repository.Verify(r => r.TryAddAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommentReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
