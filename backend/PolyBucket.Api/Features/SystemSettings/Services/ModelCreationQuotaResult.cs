namespace PolyBucket.Api.Features.SystemSettings.Services;

public enum ModelCreationQuotaKind
{
    Allowed,
    SiteLimitReached,
    UserLimitReached
}

public sealed record ModelCreationQuotaOutcome(ModelCreationQuotaKind Kind, string? Message = null)
{
    public static ModelCreationQuotaOutcome Allowed { get; } = new(ModelCreationQuotaKind.Allowed);

    public static ModelCreationQuotaOutcome SiteLimit() =>
        new(ModelCreationQuotaKind.SiteLimitReached, "The site-wide model limit has been reached.");

    public static ModelCreationQuotaOutcome UserLimit() =>
        new(ModelCreationQuotaKind.UserLimitReached, "You have reached the maximum number of models allowed for your account.");
}