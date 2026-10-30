namespace PolyBucket.Api.Features.ACL.CanUserModerateUser.Http;

public class ModerationCheckResponseDto
{
    public bool CanModerate { get; set; }
    public string ModeratorRole { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
