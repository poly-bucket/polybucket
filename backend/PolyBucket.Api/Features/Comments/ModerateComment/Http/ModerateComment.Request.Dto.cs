using System.ComponentModel.DataAnnotations;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.ModerateComment.Http;

public class ModerateCommentRequest
{
    [Required]
    [StringLength(CommentLimits.MaxReasonLength, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
