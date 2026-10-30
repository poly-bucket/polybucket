using PolyBucket.Api.Features.Files.GetFileConfig.Http;
        using PolyBucket.Api.Features.Files.GetFileConfig.Repository;

        namespace PolyBucket.Api.Features.Files.GetFileConfig.Domain;

        public interface IGetFileConfigService
        {
            Task<FileConfigResponse> GetFileConfigAsync(CancellationToken cancellationToken = default);
        }

        public class GetFileConfigService(IGetFileConfigRepository repository) : IGetFileConfigService
        {
            public async Task<FileConfigResponse> GetFileConfigAsync(CancellationToken cancellationToken = default)
            {
                var fileTypes = await repository.GetEnabledFileTypesAsync(cancellationToken);
                var categories = fileTypes
                    .GroupBy(ft => ft.Category)
                    .ToDictionary(g => g.Key, g => g.OrderBy(ft => ft.Priority).ToList());
                var storageSettings = await repository.GetEffectiveStorageSettingsAsync(cancellationToken);

                return new FileConfigResponse
                {
                    StorageProvider = storageSettings.IsConfigured ? storageSettings.Provider : "unconfigured",
                    MaxTotalFileSize = fileTypes.Count > 0 ? fileTypes.Max(ft => ft.MaxFileSizeBytes) : 100L * 1024 * 1024,
                    MaxFilesPerUpload = fileTypes.Count > 0 ? fileTypes.Max(ft => ft.MaxPerUpload) : 10,
                    SupportedExtensions = fileTypes.Select(ft => ft.Extension).ToList(),
                    FileTypes = categories,
                    GlobalSettings = new GlobalFileSettings()
                };
            }
        }
