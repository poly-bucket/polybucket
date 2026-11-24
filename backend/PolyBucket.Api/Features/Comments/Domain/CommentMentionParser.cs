using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PolyBucket.Api.Features.Comments.Domain;

public static partial class CommentMentionParser
{
    [GeneratedRegex(@"(?<![\w.@])(@)([a-zA-Z0-9_]{3,50})(?![\w.@])", RegexOptions.CultureInvariant)]
    private static partial Regex MentionRegex();

    public static IReadOnlyList<string> ExtractUsernames(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var matches = MentionRegex().Matches(content);
        var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        var usernames = new List<string>();

        foreach (Match match in matches)
        {
            var username = match.Groups[2].Value;
            if (seen.Add(username))
            {
                usernames.Add(username);
            }
        }

        return usernames;
    }
}