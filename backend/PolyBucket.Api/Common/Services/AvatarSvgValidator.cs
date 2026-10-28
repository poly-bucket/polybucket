using System;
using System.Text.RegularExpressions;

namespace PolyBucket.Api.Common.Services;

public interface IAvatarSvgValidator
{
    bool TryValidate(string? value, out string? sanitizedSvg, out string? errorMessage);
}

public partial class AvatarSvgValidator : IAvatarSvgValidator
{
    public const int MaxSvgLength = 256_000;

    public bool TryValidate(string? value, out string? sanitizedSvg, out string? errorMessage)
    {
        sanitizedSvg = null;
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Avatar must be SVG markup, not a URL.";
            return false;
        }

        if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Data URLs are not allowed for avatars.";
            return false;
        }

        if (trimmed.Length > MaxSvgLength)
        {
            errorMessage = "Avatar SVG is too large.";
            return false;
        }

        if (!trimmed.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Avatar must be SVG markup.";
            return false;
        }

        var withoutScript = ScriptBlock().Replace(trimmed, string.Empty);
        var withoutForeign = ForeignObjectBlock().Replace(withoutScript, string.Empty);
        var withoutHandlers = EventHandlerAttr().Replace(withoutForeign, string.Empty);

        if (!withoutHandlers.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Invalid avatar SVG.";
            return false;
        }

        sanitizedSvg = withoutHandlers;
        return true;
    }

    [GeneratedRegex("<script[\\s\\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptBlock();

    [GeneratedRegex("<foreignObject[\\s\\S]*?</foreignObject>", RegexOptions.IgnoreCase)]
    private static partial Regex ForeignObjectBlock();

    [GeneratedRegex("\\s(on[a-z]+)\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerAttr();
}
