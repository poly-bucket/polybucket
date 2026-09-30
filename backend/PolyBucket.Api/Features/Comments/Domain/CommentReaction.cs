using System;

namespace PolyBucket.Api.Features.Comments.Domain;

public enum CommentReactionType
{
    Like,
    Dislike
}

public class CommentReaction
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public virtual EnhancedComment Comment { get; set; } = null!;
    public Guid UserId { get; set; }
    public CommentReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
