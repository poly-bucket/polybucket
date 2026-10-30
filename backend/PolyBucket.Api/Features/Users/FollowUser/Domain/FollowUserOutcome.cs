namespace PolyBucket.Api.Features.Users.FollowUser.Domain;

public enum FollowUserOutcome
{
    Success,
    AlreadyFollowing,
    SelfFollow,
    TargetNotFound,
    TargetPrivate,
    TargetBanned
}
