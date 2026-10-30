using Microsoft.EntityFrameworkCore;
        using PolyBucket.Database.Persistence;

        namespace PolyBucket.Api.Features.Files.GetSupportedExtensions.Repository;

        public class GetSupportedExtensionsRepository(PolyBucketDbContext context) : IGetSupportedExtensionsRepository
        {
            public Task<List<string>> GetExtensionsAsync(CancellationToken cancellationToken = default) =>
                context.FileTypeSettings
                    .Where(ft => ft.Enabled)
                    .OrderBy(ft => ft.FileExtension)
                    .Select(ft => ft.FileExtension)
                    .ToListAsync(cancellationToken);
        }
