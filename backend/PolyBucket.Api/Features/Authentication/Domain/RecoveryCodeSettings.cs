namespace PolyBucket.Api.Features.Authentication.Domain;

public sealed class RecoveryCodeSettings
{
    public const string SectionName = "RecoveryCodes";

    public int CodeCount { get; set; } = 10;

    public int CodeLength { get; set; } = 12;
}
