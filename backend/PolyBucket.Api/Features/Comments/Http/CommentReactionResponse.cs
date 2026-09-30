using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.Http;

public class CommentReactionResponse
{
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public bool UserHasLiked { get; set; }
    public bool UserHasDisliked { get; set; }
    public bool Changed { get; set; }

    public static CommentReactionResponse From(CommentReactionOutcome outcome) => new()
    {
        Likes = outcome.Likes,
        Dislikes = outcome.Dislikes,
        UserHasLiked = outcome.UserReaction == CommentReactionType.Like,
        UserHasDisliked = outcome.UserReaction == CommentReactionType.Dislike,
        Changed = outcome.Change == CommentReactionChange.Applied
    };
}
