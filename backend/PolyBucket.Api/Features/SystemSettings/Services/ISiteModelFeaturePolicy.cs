using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteModelFeaturePolicy
{
    Task<FeatureAccessLevel> GetCommentsAccessLevelAsync(CancellationToken cancellationToken = default);

    Task<FeatureAccessLevel> GetCollectionsAccessLevelAsync(CancellationToken cancellationToken = default);

    Task<bool> CanReadCommentsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<bool> CanWriteCommentsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<bool> CanReadCollectionsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<bool> CanWriteCollectionsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<ModelCreationQuotaOutcome> ValidateModelCreationQuotaAsync(Guid authorId, CancellationToken cancellationToken = default);

    void Invalidate();
}

public sealed record SiteModelFeaturePolicySnapshot(
    bool EnableModelComments,
    FeatureAccessLevel CommentsAccess,
    bool EnableModelCollections,
    FeatureAccessLevel CollectionsAccess,
    int LimitTotalModels,
    int MaxModelsPerUser)
{
    public static SiteModelFeaturePolicySnapshot FailClosed { get; } = new(
        false,
        FeatureAccessLevel.Hidden,
        false,
        FeatureAccessLevel.Hidden,
        0,
        0);
}