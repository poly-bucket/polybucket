using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Repository;
using PolyBucket.Api.Features.Authentication.Services;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Domain;

public class GetPendingRecoveryCodesDisplayQueryHandler(
    IHttpContextAccessor httpContextAccessor,
    IGetPendingRecoveryCodesDisplayRepository repository,
    IRecoveryCodePresentationService presentationService)
{
    public async Task<GetPendingRecoveryCodesDisplayResponse?> Handle(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await repository.GetUserWithRoleAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        if (user.Role?.Name != "Admin" || user.HasCompletedFirstTimeSetup)
        {
            return null;
        }

        var codes = presentationService.TryGetPendingRecoveryCodes(user);
        if (codes == null || codes.Count == 0)
        {
            return null;
        }

        return new GetPendingRecoveryCodesDisplayResponse { RecoveryCodes = codes };
    }

    private Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }
}
