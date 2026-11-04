using PolyBucket.Database.Entities.SystemSettings;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public static class AllowedFileTypesAlignment
{
    public static string BuildDerivedAllowedFileTypes(IEnumerable<FileTypeSettings> enabledTypes)
    {
        return string.Join(
            ",",
            enabledTypes
                .Where(ft => ft.Enabled && ft.DeletedAt == null)
                .OrderBy(ft => ft.Category)
                .ThenBy(ft => ft.Priority)
                .ThenBy(ft => ft.FileExtension)
                .Select(ft => ft.FileExtension.Trim())
                .Where(ext => !string.IsNullOrEmpty(ext)));
    }

    public static HashSet<string> ParseExtensionSet(string? commaSeparated)
    {
        if (string.IsNullOrWhiteSpace(commaSeparated))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return commaSeparated
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(ext => !string.IsNullOrWhiteSpace(ext))
            .Select(NormalizeExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static bool AreExtensionSetsInSync(string? siteAllowedFileTypes, string derivedAllowedFileTypes)
    {
        var siteSet = ParseExtensionSet(siteAllowedFileTypes);
        var derivedSet = ParseExtensionSet(derivedAllowedFileTypes);
        return siteSet.SetEquals(derivedSet);
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
