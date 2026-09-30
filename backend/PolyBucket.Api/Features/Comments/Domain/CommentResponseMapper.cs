using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Comments.Repository;

namespace PolyBucket.Api.Features.Comments.Domain;

public class CommentResponseMapper(IEnhancedCommentsPlugin commentsPlugin, ICommentReactionRepository reactionRepository) : ICommentResponseMapper
{
    public async Task<List<CommentResponse>> MapAsync(
        IReadOnlyList<EnhancedComment> comments,
        Guid? currentUserId,
        bool isAdmin,
        bool includeReplies,
        CancellationToken cancellationToken = default)
    {
        var replies = new Dictionary<Guid, List<EnhancedComment>>();
        if (includeReplies)
        {
            foreach (var comment in comments.Where(c => c.ParentCommentId == null))
            {
                replies[comment.Id] = (await commentsPlugin.GetRepliesAsync(comment.Id)).ToList();
            }
        }

        var allIds = comments.Select(c => c.Id)
            .Concat(replies.Values.SelectMany(r => r).Select(r => r.Id))
            .Distinct()
            .ToList();

        IReadOnlyDictionary<Guid, CommentReactionType> reactions = currentUserId.HasValue
            ? await reactionRepository.GetUserReactionsAsync(allIds, currentUserId.Value, cancellationToken)
            : new Dictionary<Guid, CommentReactionType>();

        return comments
            .Select(c => Map(c, currentUserId, isAdmin, reactions,
                replies.TryGetValue(c.Id, out var children)
                    ? children.Select(r => Map(r, currentUserId, isAdmin, reactions, new List<CommentResponse>())).ToList()
                    : new List<CommentResponse>()))
            .ToList();
    }

    private static CommentResponse Map(
        EnhancedComment comment,
        Guid? currentUserId,
        bool isAdmin,
        IReadOnlyDictionary<Guid, CommentReactionType> reactions,
        List<CommentResponse> replies)
    {
        reactions.TryGetValue(comment.Id, out var reaction);
        var hasReaction = reactions.ContainsKey(comment.Id);
        return new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            AuthorUsername = comment.Author?.Username ?? "Unknown",
            Target = comment.GetTarget(),
            Likes = comment.Likes,
            Dislikes = comment.Dislikes,
            IsEdited = comment.IsEdited,
            IsModerated = comment.IsModerated,
            IsHidden = comment.IsHidden,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            LastEditedAt = comment.LastEditedAt,
            UserHasLiked = hasReaction && reaction == CommentReactionType.Like,
            UserHasDisliked = hasReaction && reaction == CommentReactionType.Dislike,
            CanEdit = currentUserId.HasValue && comment.CanBeEditedBy(currentUserId.Value),
            CanDelete = currentUserId.HasValue && comment.CanBeDeletedBy(currentUserId.Value, isAdmin),
            Replies = replies
        };
    }
}
