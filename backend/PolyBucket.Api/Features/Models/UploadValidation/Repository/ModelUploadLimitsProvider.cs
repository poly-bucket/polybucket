using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Models.UploadValidation.Repository;

public class ModelUploadLimitsProvider(PolyBucketDbContext context) : IModelUploadLimitsProvider
{
  private readonly PolyBucketDbContext _context = context;

  public async Task<ModelUploadLimitsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
  {
    var systemSetup = await _context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    var siteMaxFileSize = systemSetup?.MaxFileSizeBytes ?? ModelUploadLimitsSnapshot.DefaultSiteMaxFileSizeBytes;
    var maxFilesPerUpload = systemSetup?.MaxFilesPerUpload ?? ModelUploadLimitsSnapshot.DefaultMaxFilesPerUpload;

    var allTypeRows = await _context.FileTypeSettings
      .AsNoTracking()
      .Where(ft => ft.DeletedAt == null)
      .ToListAsync(cancellationToken);

    var extensionCategories = allTypeRows
      .Select(ft => new { Ext = NormalizeExtension(ft.FileExtension), ft.Category })
      .Where(x => !string.IsNullOrEmpty(x.Ext))
      .GroupBy(x => x.Ext, StringComparer.OrdinalIgnoreCase)
      .ToDictionary(g => g.Key, g => g.First().Category ?? string.Empty, StringComparer.OrdinalIgnoreCase);

    var enabledTypes = allTypeRows
      .Where(ft => ft.Enabled)
      .Select(ft => new ModelUploadFileTypeLimit
      {
        Extension = NormalizeExtension(ft.FileExtension),
        Category = ft.Category ?? string.Empty,
        MaxFileSizeBytes = ft.MaxFileSizeBytes
      })
      .Where(t => !string.IsNullOrEmpty(t.Extension))
      .ToList();

    if (enabledTypes.Count == 0)
    {
      var allowedFromSite = systemSetup?.AllowedFileTypes ?? string.Empty;
      var parsed = AllowedFileTypesAlignment.ParseExtensionSet(allowedFromSite);
      enabledTypes = parsed
        .Select(ext => new ModelUploadFileTypeLimit
        {
          Extension = ext,
          Category = extensionCategories.TryGetValue(ext, out var cat) ? cat : string.Empty,
          MaxFileSizeBytes = siteMaxFileSize
        })
        .ToList();
    }

    return new ModelUploadLimitsSnapshot
    {
      SiteMaxFileSizeBytes = siteMaxFileSize,
      MaxFilesPerUpload = maxFilesPerUpload,
      EnabledFileTypes = enabledTypes,
      ExtensionCategories = extensionCategories
    };
  }

  private static string NormalizeExtension(string extension)
  {
    var trimmed = extension.Trim();
    if (string.IsNullOrEmpty(trimmed))
    {
      return trimmed;
    }

    return trimmed.StartsWith('.') ? trimmed.ToLowerInvariant() : $".{trimmed.ToLowerInvariant()}";
  }
}