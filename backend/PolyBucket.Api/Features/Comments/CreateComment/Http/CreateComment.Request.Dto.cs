using System;
using System.ComponentModel.DataAnnotations;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.CreateComment.Http;

public class CreateCommentRequest
{
    [Required]
    public CommentTarget Target { get; set; } = new();

    [Required]
    [StringLength(CommentLimits.MaxContentLength, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }
}
