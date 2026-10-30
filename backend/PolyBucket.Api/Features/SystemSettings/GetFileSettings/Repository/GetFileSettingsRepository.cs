using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Repository;

public class GetFileSettingsRepository(PolyBucketDbContext context) : IGetFileSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<IReadOnlyList<FileTypeSettingsData>> GetFileTypesAsync(CancellationToken cancellationToken)
    {
        return await _context.FileTypeSettings
            .Where(ft => ft.DeletedAt == null)
            .OrderBy(ft => ft.Category)
            .ThenBy(ft => ft.Priority)
            .ThenBy(ft => ft.FileExtension)
            .Select(ft => new FileTypeSettingsData
            {
                Id = ft.Id,
                FileExtension = ft.FileExtension,
                Enabled = ft.Enabled,
                MaxFileSizeBytes = ft.MaxFileSizeBytes,
                MaxPerUpload = ft.MaxPerUpload,
                DisplayName = ft.DisplayName,
                Description = ft.Description,
                MimeType = ft.MimeType,
                RequiresPreview = ft.RequiresPreview,
                IsCompressible = ft.IsCompressible,
                Category = ft.Category,
                Priority = ft.Priority,
                IsDefault = ft.IsDefault
            })
            .ToListAsync(cancellationToken);
    }
}
