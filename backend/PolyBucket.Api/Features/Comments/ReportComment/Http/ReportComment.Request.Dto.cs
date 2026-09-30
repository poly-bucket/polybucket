using System.ComponentModel.DataAnnotations;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.ReportComment.Http;

public class ReportCommentRequest
{
    [Required]
    [StringLength(CommentLimits.MaxReasonLength, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
