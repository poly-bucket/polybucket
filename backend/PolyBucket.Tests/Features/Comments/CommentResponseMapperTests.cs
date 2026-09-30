using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments;

public class CommentResponseMapperTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Mock<ICommentReactionRepository> _reactions = new();
    private readonly Guid _viewerId = Guid.NewGuid();

    private static EnhancedComment CreateComment(Guid authorId, Guid? parentId = null) => new()
    {
        Id = Guid.NewGuid(),
        Content = "Nice print",
        AuthorId = authorId,
        Author = new User { Id = authorId, Username = "maker", Email = "maker@example.com" },
        ParentCommentId = parentId,
        CreatedAt = DateTime.UtcNow
    };

    [Fact(DisplayName = "When mapping comments, the viewer's likes and dislikes on comments and replies are filled in with one lookup.")]
    public async Task MapAsync_PopulatesViewerReactions()
    {
        // Arrange
        var liked = CreateComment(Guid.NewGuid());
        var reply = CreateComment(Guid.NewGuid(), liked.Id);
        var neutral = CreateComment(Guid.NewGuid());
        _plugin.Setup(p => p.GetRepliesAsync(liked.Id, false)).ReturnsAsync(new[] { reply });
        _plugin.Setup(p => p.GetRepliesAsync(neutral.Id, false)).ReturnsAsync(Array.Empty<EnhancedComment>());
        _reactions.Setup(r => r.GetUserReactionsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), _viewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentReactionType> { [liked.Id] = CommentReactionType.Like, [reply.Id] = CommentReactionType.Dislike });
        var mapper = new CommentResponseMapper(_plugin.Object, _reactions.Object);

        // Act
        var mapped = await mapper.MapAsync(new[] { liked, neutral }, _viewerId, isAdmin: false, includeReplies: true);

        // Assert
        mapped[0].UserHasLiked.ShouldBeTrue();
        mapped[0].UserHasDisliked.ShouldBeFalse();
        mapped[0].Replies.Single().UserHasDisliked.ShouldBeTrue();
        mapped[1].UserHasLiked.ShouldBeFalse();
        mapped[1].UserHasDisliked.ShouldBeFalse();
        _reactions.Verify(r => r.GetUserReactionsAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 3), _viewerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the author views their comment, they can edit and delete it; other users cannot.")]
    public async Task MapAsync_SetsEditAndDeletePermissions()
    {
        // Arrange
        var own = CreateComment(_viewerId);
        var other = CreateComment(Guid.NewGuid());
        _reactions.Setup(r => r.GetUserReactionsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), _viewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CommentReactionType>());
        var mapper = new CommentResponseMapper(_plugin.Object, _reactions.Object);

        // Act
        var mapped = await mapper.MapAsync(new[] { own, other }, _viewerId, isAdmin: false, includeReplies: false);

        // Assert
        mapped[0].CanEdit.ShouldBeTrue();
        mapped[0].CanDelete.ShouldBeTrue();
        mapped[1].CanEdit.ShouldBeFalse();
        mapped[1].CanDelete.ShouldBeFalse();
    }

    [Fact(DisplayName = "When there is no viewer, no reaction lookup is made and nothing is editable.")]
    public async Task MapAsync_Anonymous_SkipsReactionLookup()
    {
        // Arrange
        var comment = CreateComment(Guid.NewGuid());
        var mapper = new CommentResponseMapper(_plugin.Object, _reactions.Object);

        // Act
        var mapped = await mapper.MapAsync(new[] { comment }, null, isAdmin: false, includeReplies: false);

        // Assert
        mapped.Single().CanEdit.ShouldBeFalse();
        _reactions.VerifyNoOtherCalls();
    }
}
