using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Domain;

public class RegenerateRecoveryCodesCommandHandler(
    IHttpContextAccessor httpContextAccessor,
    IRegenerateRecoveryCodesRepository repository,
    IRecoveryCodeProvisioningService provisioningService)
{
    public async Task<RegenerateRecoveryCodesResponse> Handle(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await repository.GetUserAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException();

        var codes = await provisioningService.ProvisionForNewUserAsync(
            user,
            userId,
            storePendingDisplayPayload: false,
            UserAuditAction.RecoveryCodesRegenerated,
            cancellationToken);

        return new RegenerateRecoveryCodesResponse { RecoveryCodes = codes };
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
