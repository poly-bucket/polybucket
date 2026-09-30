using System.ComponentModel.DataAnnotations;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.UpdateComment.Http;

public class UpdateCommentContentRequest
{
    [Required]
    [StringLength(CommentLimits.MaxContentLength, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
