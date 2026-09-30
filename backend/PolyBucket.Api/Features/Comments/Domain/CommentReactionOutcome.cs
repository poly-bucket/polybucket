namespace PolyBucket.Api.Features.Comments.Domain;

public enum CommentReactionChange
{
    Applied,
    Unchanged,
    NotFound,
    Forbidden
}

public sealed record CommentReactionOutcome(
    CommentReactionChange Change,
    int Likes,
    int Dislikes,
    CommentReactionType? UserReaction)
{
    public static CommentReactionOutcome NotFound { get; } = new(CommentReactionChange.NotFound, 0, 0, null);
    public static CommentReactionOutcome Forbidden { get; } = new(CommentReactionChange.Forbidden, 0, 0, null);
}
