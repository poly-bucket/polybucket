using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteModelUploadPolicy
{
    Task<bool> IsAnonymousModelUploadAllowedAsync(CancellationToken cancellationToken = default);

    Task<bool> IsLoginRequiredForUnauthenticatedUploadAsync(CancellationToken cancellationToken = default);

    Task<PrivacySettings> GetDefaultModelPrivacyAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}