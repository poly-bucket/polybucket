namespace PolyBucket.Api.Features.Comments.Commands
{
    public class AddCommentRequest
    {
        public Guid TargetId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
} 