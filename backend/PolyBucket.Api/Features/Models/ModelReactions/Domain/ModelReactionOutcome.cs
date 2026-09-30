namespace PolyBucket.Api.Features.Models.ModelReactions.Domain;

public enum ModelReactionChange
{
    Applied,
    Unchanged,
    NotFound,
    Forbidden,
    Disabled,
    Unauthorized
}

public sealed record ModelReactionOutcome(
    ModelReactionChange Change,
    int Likes,
    int Dislikes,
    ModelReactionType? UserReaction)
{
    public static ModelReactionOutcome NotFound { get; } = new(ModelReactionChange.NotFound, 0, 0, null);
    public static ModelReactionOutcome Forbidden { get; } = new(ModelReactionChange.Forbidden, 0, 0, null);
    public static ModelReactionOutcome Disabled { get; } = new(ModelReactionChange.Disabled, 0, 0, null);
    public static ModelReactionOutcome Unauthorized { get; } = new(ModelReactionChange.Unauthorized, 0, 0, null);
}
