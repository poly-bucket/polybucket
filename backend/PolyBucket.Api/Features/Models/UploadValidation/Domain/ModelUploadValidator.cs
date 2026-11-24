using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.Models.UploadValidation.Domain;

public class ModelUploadValidator(IModelUploadLimitsProvider limitsProvider) : IModelUploadValidator
{
  private readonly IModelUploadLimitsProvider _limitsProvider = limitsProvider;

  public async Task<string?> ValidateAsync(IFormFile[] files, CancellationToken cancellationToken = default)
  {
    var snapshot = await _limitsProvider.GetSnapshotAsync(cancellationToken);
    var allowed = snapshot.AllowedExtensions;

    var typeByExtension = snapshot.EnabledFileTypes
      .GroupBy(t => t.Extension, StringComparer.OrdinalIgnoreCase)
      .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    var maxFilesAllowed = snapshot.MaxFilesPerUpload;
    foreach (var file in files)
    {
      if (file.Length == 0)
      {
        continue;
      }

      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (typeByExtension.TryGetValue(extension, out var typeLimit) && typeLimit.MaxPerUpload > 0)
      {
        maxFilesAllowed = Math.Min(maxFilesAllowed, typeLimit.MaxPerUpload);
      }
    }

    if (files.Length > maxFilesAllowed)
    {
      return $"Too many files. Maximum of {maxFilesAllowed} files allowed per upload.";
    }

    var has3D = false;
    var totalSize = 0L;

    foreach (var file in files)
    {
      if (file.Length == 0)
      {
        continue;
      }

      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (!allowed.Contains(extension))
      {
        var supportedList = allowed.OrderBy(e => e, StringComparer.OrdinalIgnoreCase);
        return $"Unsupported file type: {extension}. Supported formats: {string.Join(", ", supportedList)}";
      }

      if (!FileSignatureValidator.IsTextFile(extension))
      {
        var isValidSignature = await FileSignatureValidator.ValidateFileSignatureAsync(file, extension);
        if (!isValidSignature)
        {
          return $"File {file.FileName} does not match expected file signature for {extension}. File may be corrupted or have incorrect extension.";
        }
      }

      typeByExtension.TryGetValue(extension, out var typeLimit);
      var perTypeMax = typeLimit?.MaxFileSizeBytes ?? snapshot.SiteMaxFileSizeBytes;
      var maxSize = Math.Min(snapshot.SiteMaxFileSizeBytes, perTypeMax);
      if (file.Length > maxSize)
      {
        var maxSizeMb = maxSize / (1024 * 1024);
        return $"File {file.FileName} is too large. Maximum size: {maxSizeMb}MB";
      }

      var category = typeLimit?.Category;
      if (string.IsNullOrEmpty(category) && snapshot.ExtensionCategories.TryGetValue(extension, out var mappedCategory))
      {
        category = mappedCategory;
      }

      if (!string.IsNullOrEmpty(category) && category.Equals("3D", StringComparison.OrdinalIgnoreCase))
      {
        has3D = true;
      }

      totalSize += file.Length;
    }

    if (!has3D)
    {
      return "At least one 3D model file is required.";
    }

    if (totalSize > ModelUploadLimitsSnapshot.MaxTotalUploadBytes)
    {
      return "Total upload size exceeds 1GB limit";
    }

    return null;
  }
}