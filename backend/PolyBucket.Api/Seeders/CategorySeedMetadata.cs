using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Seeders;

public static class CategorySeedMetadata
{
    private static readonly string[] Palette =
    [
        "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#03A9F4",
        "#00BCD4", "#009688", "#4CAF50", "#8BC34A", "#CDDC39", "#FFC107",
        "#FF9800", "#FF5722", "#795548", "#607D8B", "#F44336", "#AD1457",
        "#6A1B9A", "#4527A0", "#283593", "#1565C0", "#0277BD", "#00838F",
        "#00695C", "#2E7D32", "#558B2F", "#9E9D24", "#F9A825", "#EF6C00",
        "#D84315", "#4E342E", "#37474F", "#C62828", "#880E4F", "#4A148C",
        "#1A237E", "#0D47A1", "#01579B", "#004D40", "#1B5E20", "#33691E",
        "#827717", "#FF6F00", "#BF360C", "#3E2723", "#263238", "#B71C1C",
        "#7B1FA2", "#512DA8", "#303F9F", "#1976D2", "#0288D1", "#0097A7",
        "#00796B", "#388E3C", "#689F38", "#AFB42B", "#FFA000", "#E65100",
        "#5D4037", "#455A64"
    ];

    public static (string Description, string Color) Get(ModelCategories category)
    {
        var name = category.ToString();
        var index = (int)category;
        var color = Palette[index % Palette.Length];
        var description = $"3D models and designs related to {FormatLabel(name)}.";
        return (description, color);
    }

    private static string FormatLabel(string enumName)
    {
        if (string.IsNullOrEmpty(enumName))
        {
            return enumName;
        }

        var spaced = System.Text.RegularExpressions.Regex.Replace(enumName, "([a-z])([A-Z])", "$1 $2");
        return spaced.ToLowerInvariant();
    }
}
