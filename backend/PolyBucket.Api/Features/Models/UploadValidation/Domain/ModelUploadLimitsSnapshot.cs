namespace PolyBucket.Api.Features.Models.UploadValidation.Domain;

public sealed class ModelUploadFileTypeLimit
{
    public string Extension { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public long MaxFileSizeBytes { get; init; }
    public int MaxPerUpload { get; init; }
}

public sealed class ModelUploadLimitsSnapshot
{
  public const long DefaultSiteMaxFileSizeBytes = 100L * 1024 * 1024;
  public const int DefaultMaxFilesPerUpload = 5;
  public const long MaxTotalUploadBytes = 1024L * 1024 * 1024;

  public long SiteMaxFileSizeBytes { get; init; } = DefaultSiteMaxFileSizeBytes;
  public int MaxFilesPerUpload { get; init; } = DefaultMaxFilesPerUpload;
  public IReadOnlyList<ModelUploadFileTypeLimit> EnabledFileTypes { get; init; } = [];
  public IReadOnlyDictionary<string, string> ExtensionCategories { get; init; } =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

  public HashSet<string> AllowedExtensions
  {
    get
    {
      if (EnabledFileTypes.Count > 0)
      {
        return EnabledFileTypes
          .Select(t => t.Extension)
          .ToHashSet(StringComparer.OrdinalIgnoreCase);
      }

      return ExtensionCategories.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
  }
}