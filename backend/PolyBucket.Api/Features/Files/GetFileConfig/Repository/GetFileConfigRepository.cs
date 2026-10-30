using Microsoft.EntityFrameworkCore;
        using PolyBucket.Api.Features.Files.GetFileConfig.Http;
        using PolyBucket.Api.Features.SystemSettings.Domain;
        using PolyBucket.Database.Persistence;

        namespace PolyBucket.Api.Features.Files.GetFileConfig.Repository;

        public class GetFileConfigRepository(PolyBucketDbContext context, IStorageSettingsResolver storageSettingsResolver) : IGetFileConfigRepository
        {
            public Task<List<FileTypeConfig>> GetEnabledFileTypesAsync(CancellationToken cancellationToken = default) =>
                context.FileTypeSettings
                    .Where(ft => ft.Enabled)
                    .OrderBy(ft => ft.Category)
                    .ThenBy(ft => ft.Priority)
                    .Select(ft => new FileTypeConfig
                    {
                        Extension = ft.FileExtension,
                        DisplayName = ft.DisplayName,
                        Description = ft.Description,
                        MimeType = ft.MimeType,
                        MaxFileSizeBytes = ft.MaxFileSizeBytes,
                        MaxPerUpload = ft.MaxPerUpload,
                        RequiresPreview = ft.RequiresPreview,
                        IsCompressible = ft.IsCompressible,
                        Category = ft.Category,
                        Priority = ft.Priority
                    })
                    .ToListAsync(cancellationToken);

            public Task<EffectiveStorageSettings> GetEffectiveStorageSettingsAsync(CancellationToken cancellationToken = default) =>
                storageSettingsResolver.GetEffectiveSettingsAsync();
        }
