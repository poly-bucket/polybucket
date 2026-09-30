namespace PolyBucket.Api.Features.Comments.Domain;

public enum CommentReactionChange
{
    Applied,
    Unchanged,
    NotFound
}

public sealed record CommentReactionOutcome(
    CommentReactionChange Change,
    int Likes,
    int Dislikes,
    CommentReactionType? UserReaction)
{
    public static CommentReactionOutcome NotFound { get; } = new(CommentReactionChange.NotFound, 0, 0, null);
}
