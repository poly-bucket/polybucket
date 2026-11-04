using System.Text;

namespace PolyBucket.Api.Features.Authentication.Services;

public static class RecoveryCodeNormalizer
{
    public static string Normalize(string rawCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawCode);
        var trimmed = rawCode.Trim();
        var builder = new StringBuilder(trimmed.Length);
        foreach (var ch in trimmed)
        {
            if (ch is '-' or ' ' or '_')
            {
                continue;
            }

            builder.Append(char.ToUpperInvariant(ch));
        }

        if (builder.Length == 0)
        {
            throw new ArgumentException("Recovery code is empty after normalization.", nameof(rawCode));
        }

        return builder.ToString();
    }
}
