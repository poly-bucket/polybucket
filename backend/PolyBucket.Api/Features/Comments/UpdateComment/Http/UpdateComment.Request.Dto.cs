using System.ComponentModel.DataAnnotations;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.UpdateComment.Http;

public class UpdateCommentContentRequest
{
    [Required]
    [StringLength(CommentLimits.MaxContentLength, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
