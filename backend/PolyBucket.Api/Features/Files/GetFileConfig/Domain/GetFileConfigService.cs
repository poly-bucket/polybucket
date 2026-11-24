using PolyBucket.Api.Features.Files.GetFileConfig.Http;
using PolyBucket.Api.Features.Files.GetFileConfig.Repository;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;

namespace PolyBucket.Api.Features.Files.GetFileConfig.Domain;

public interface IGetFileConfigService
{
    Task<FileConfigResponse> GetFileConfigAsync(CancellationToken cancellationToken = default);
}

public class GetFileConfigService(
    IGetFileConfigRepository repository,
    IModelUploadLimitsProvider uploadLimitsProvider) : IGetFileConfigService
{
    public async Task<FileConfigResponse> GetFileConfigAsync(CancellationToken cancellationToken = default)
    {
        var fileTypes = await repository.GetEnabledFileTypesAsync(cancellationToken);
        var categories = fileTypes
            .GroupBy(ft => ft.Category)
            .ToDictionary(g => g.Key, g => g.OrderBy(ft => ft.Priority).ToList());
        var storageSettings = await repository.GetEffectiveStorageSettingsAsync(cancellationToken);
        var limits = await uploadLimitsProvider.GetSnapshotAsync(cancellationToken);

        return new FileConfigResponse
        {
            StorageProvider = storageSettings.IsConfigured ? storageSettings.Provider : "unconfigured",
            MaxTotalFileSize = limits.SiteMaxFileSizeBytes,
            MaxFilesPerUpload = limits.MaxFilesPerUpload,
            SupportedExtensions = fileTypes.Select(ft => ft.Extension).ToList(),
            FileTypes = categories,
            GlobalSettings = new GlobalFileSettings()
        };
    }
}
