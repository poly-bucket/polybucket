using System.ComponentModel.DataAnnotations;
using PolyBucket.Database.Entities.Comments;

namespace PolyBucket.Api.Features.Comments.ModerateAllUserComments.Http;

public class ModerateAllUserCommentsRequest
{
    [Required]
    [StringLength(CommentLimits.MaxReasonLength, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
