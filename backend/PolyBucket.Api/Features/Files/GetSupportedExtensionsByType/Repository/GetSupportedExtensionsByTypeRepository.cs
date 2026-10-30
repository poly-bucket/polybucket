using Microsoft.EntityFrameworkCore;
        using PolyBucket.Database.Persistence;

        namespace PolyBucket.Api.Features.Files.GetSupportedExtensionsByType.Repository;

        public class GetSupportedExtensionsByTypeRepository(PolyBucketDbContext context) : IGetSupportedExtensionsByTypeRepository
        {
            public Task<List<string>> GetExtensionsForTypeAsync(string fileType, CancellationToken cancellationToken = default) =>
                context.FileTypeSettings
                    .Where(ft => ft.Enabled && ft.Category.ToLower() == fileType.ToLower())
                    .OrderBy(ft => ft.Priority)
                    .ThenBy(ft => ft.FileExtension)
                    .Select(ft => ft.FileExtension)
                    .ToListAsync(cancellationToken);
        }
