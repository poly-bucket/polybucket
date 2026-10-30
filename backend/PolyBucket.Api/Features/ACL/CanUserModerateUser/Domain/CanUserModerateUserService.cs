using PolyBucket.Api.Features.ACL.CanUserModerateUser.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.CanUserModerateUser.Domain;

public interface ICanUserModerateUserService
{
    Task<ModerationCheckResponseDto> CanUserModerateUserAsync(Guid moderatorId, Guid targetUserId, CancellationToken cancellationToken = default);
}

public class CanUserModerateUserService(IPermissionService permissionService) : ICanUserModerateUserService
{
    public async Task<ModerationCheckResponseDto> CanUserModerateUserAsync(Guid moderatorId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var canModerate = await permissionService.CanUserModerateUserAsync(moderatorId, targetUserId);
        var moderatorRole = await permissionService.GetUserRoleAsync(moderatorId);
        var targetRole = await permissionService.GetUserRoleAsync(targetUserId);

        return new ModerationCheckResponseDto
        {
            CanModerate = canModerate,
            ModeratorRole = moderatorRole?.Name ?? "Unknown",
            TargetRole = targetRole?.Name ?? "Unknown",
            Reason = canModerate
                ? "User has sufficient moderation privileges"
                : "User lacks moderation privileges or target user has equal/higher role"
        };
    }
}
