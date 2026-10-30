using PolyBucket.Api.Features.Files.GetFileConfig.Http;
        using PolyBucket.Api.Features.SystemSettings.Domain;

        namespace PolyBucket.Api.Features.Files.GetFileConfig.Repository;

        public interface IGetFileConfigRepository
        {
            Task<List<FileTypeConfig>> GetEnabledFileTypesAsync(CancellationToken cancellationToken = default);
            Task<EffectiveStorageSettings> GetEffectiveStorageSettingsAsync(CancellationToken cancellationToken = default);
        }
