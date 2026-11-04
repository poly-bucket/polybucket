using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.Services;

public interface IRecoveryCodePresentationService
{
    IReadOnlyList<string>? TryGetPendingRecoveryCodes(User user);
}

public sealed class RecoveryCodePresentationService(IRecoveryCodeService recoveryCodeService) : IRecoveryCodePresentationService
{
    public IReadOnlyList<string>? TryGetPendingRecoveryCodes(User user)
    {
        if (user.RecoveryCodesAcknowledgedAt != null
            || string.IsNullOrEmpty(user.ProtectedPendingRecoveryCodesPayload))
        {
            return null;
        }

        return recoveryCodeService.DeserializeDisplayPayload(user.ProtectedPendingRecoveryCodesPayload);
    }
}
