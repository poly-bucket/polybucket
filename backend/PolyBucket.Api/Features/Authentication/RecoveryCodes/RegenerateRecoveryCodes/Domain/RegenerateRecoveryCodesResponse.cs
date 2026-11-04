namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Domain;

public sealed class RegenerateRecoveryCodesResponse
{
    public IReadOnlyList<string> RecoveryCodes { get; set; } = Array.Empty<string>();
}
