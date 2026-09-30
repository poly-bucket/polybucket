using System;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Comments.Http;

public static class CommentsClaimsPrincipalExtensions
{
    public static Guid? GetCommentUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    public static bool IsCommentAdmin(this ClaimsPrincipal principal) => principal.IsInRole("Admin");

    public static bool IsCommentModerator(this ClaimsPrincipal principal) =>
        principal.IsInRole("Admin") || principal.IsInRole("Moderator");
}
