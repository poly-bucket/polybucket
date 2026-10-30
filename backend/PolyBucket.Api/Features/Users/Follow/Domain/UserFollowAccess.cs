using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Users.Follow.Domain;

public static class UserFollowAccess
{
    public static bool CanViewFollowLists(User target, Guid? requestingUserId, bool isRequestingUserAdmin)
    {
        if (target.IsProfilePublic)
        {
            return true;
        }

        if (isRequestingUserAdmin)
        {
            return true;
        }

        return requestingUserId.HasValue && requestingUserId.Value == target.Id;
    }
}
