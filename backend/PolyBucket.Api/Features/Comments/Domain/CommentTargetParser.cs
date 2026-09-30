using System;

namespace PolyBucket.Api.Features.Comments.Domain;

public static class CommentTargetParser
{
    public static bool TryParse(string? targetType, Guid targetId, out CommentTarget target)
    {
        CommentTargetType? parsed = targetType?.Trim().ToLowerInvariant() switch
        {
            "model" => CommentTargetType.Model,
            "user" or "userprofile" => CommentTargetType.UserProfile,
            "collection" => CommentTargetType.Collection,
            "report" => CommentTargetType.Report,
            _ => null
        };

        target = new CommentTarget { TargetId = targetId, TargetType = parsed ?? CommentTargetType.Model };
        return parsed.HasValue;
    }
}
