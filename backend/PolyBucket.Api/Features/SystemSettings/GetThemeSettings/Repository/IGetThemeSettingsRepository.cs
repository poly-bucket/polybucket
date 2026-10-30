using PolyBucket.Database.Entities.SystemSettings;

namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Repository;

public interface IGetThemeSettingsRepository
{
    Task<SystemSetup?> GetSystemSetupAsync(CancellationToken cancellationToken);
}
