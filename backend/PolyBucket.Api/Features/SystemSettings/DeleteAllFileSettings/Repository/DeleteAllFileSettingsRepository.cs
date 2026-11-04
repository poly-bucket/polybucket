using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Repository
{
    public class DeleteAllFileSettingsRepository(PolyBucketDbContext context) : IDeleteAllFileSettingsRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<int> DeleteAllFileSettingsAndReturnCountAsync(
            Guid deletedById,
            CancellationToken cancellationToken = default)
        {
            var fileTypes = await _context.FileTypeSettings
                .Where(ft => ft.DeletedAt == null)
                .ToListAsync(cancellationToken);

            if (fileTypes.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var fileType in fileTypes)
            {
                fileType.DeletedAt = now;
                fileType.DeletedById = deletedById;
                fileType.UpdatedAt = now;
                fileType.UpdatedById = deletedById;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return fileTypes.Count;
        }
    }
}
