namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Domain;

public sealed class GetPendingRecoveryCodesDisplayResponse
{
    public IReadOnlyList<string> RecoveryCodes { get; set; } = Array.Empty<string>();
}
