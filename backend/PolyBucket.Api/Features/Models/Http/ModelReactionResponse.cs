using PolyBucket.Api.Features.Models.ModelReactions.Domain;

namespace PolyBucket.Api.Features.Models.Http;

public class ModelReactionResponse
{
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public bool UserHasLiked { get; set; }
    public bool UserHasDisliked { get; set; }
    public bool Changed { get; set; }

    public static ModelReactionResponse From(ModelReactionOutcome outcome) => new()
    {
        Likes = outcome.Likes,
        Dislikes = outcome.Dislikes,
        UserHasLiked = outcome.UserReaction == ModelReactionType.Like,
        UserHasDisliked = outcome.UserReaction == ModelReactionType.Dislike,
        Changed = outcome.Change == ModelReactionChange.Applied
    };
}
