using System.Security.Claims;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Models.Common;

public static class ModelVersionExposure
{
    public static async Task<bool> ShouldExposeVersionHistoryAsync(
        ISiteModelVersioningPolicy policy,
        ClaimsPrincipal? user,
        Guid modelAuthorId,
        CancellationToken cancellationToken)
    {
        if (await policy.IsModelVersioningEnabledAsync(cancellationToken))
        {
            return true;
        }

        if (user == null)
        {
            return false;
        }

        return await policy.CanViewVersionHistoryWhenDisabledAsync(user, modelAuthorId, cancellationToken);
    }
}