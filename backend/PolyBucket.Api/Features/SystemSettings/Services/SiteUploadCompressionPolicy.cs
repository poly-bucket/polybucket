using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteUploadCompressionPolicy(PolyBucketDbContext context) : ISiteUploadCompressionPolicy
{
    public async Task<bool> ShouldCompressAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var systemSetup = await context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (systemSetup is not { EnableFileCompression: true })
        {
            return false;
        }

        var extension = GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        var extensionNoDot = extension.TrimStart('.');

        var fileTypes = await context.FileTypeSettings
            .AsNoTracking()
            .Where(ft => ft.DeletedAt == null && ft.Enabled && ft.IsCompressible)
            .ToListAsync(cancellationToken);

        return fileTypes.Any(ft =>
        {
            var normalized = ft.FileExtension.Trim().TrimStart('.').ToLowerInvariant();
            return normalized == extensionNoDot;
        });
    }

    private static string GetExtension(string fileName)
    {
        var lastDot = fileName.LastIndexOf('.');
        if (lastDot < 0 || lastDot == fileName.Length - 1)
        {
            return string.Empty;
        }

        return fileName[lastDot..].ToLowerInvariant();
    }
}