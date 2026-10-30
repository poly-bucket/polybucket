using PolyBucket.Database.Entities.SystemSettings;

namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Repository;

public interface IUpdateThemeSettingsRepository
{
    Task<SystemSetup?> GetSystemSetupAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
