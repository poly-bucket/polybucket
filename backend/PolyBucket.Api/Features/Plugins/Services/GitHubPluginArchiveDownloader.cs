namespace PolyBucket.Api.Features.Plugins.Services;

public class GitHubPluginArchiveDownloader(
    HttpClient httpClient,
    ILogger<GitHubPluginArchiveDownloader> logger) : IPluginArchiveDownloader
{
    public async Task<PluginArchiveDownloadResult> DownloadRepositoryArchiveAsync(
        string repositoryUrl,
        string branch,
        CancellationToken cancellationToken = default)
    {
        if (!PluginRepositoryArchiveHelper.TryBuildGitArchiveUrl(repositoryUrl, branch, out var archiveUrl, out var error))
        {
            return new PluginArchiveDownloadResult { Success = false, Message = error };
        }

        try
        {
            var bytes = await httpClient.GetByteArrayAsync(archiveUrl, cancellationToken);
            return new PluginArchiveDownloadResult { Success = true, ArchiveBytes = bytes };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download plugin archive from {ArchiveUrl}", archiveUrl);
            return new PluginArchiveDownloadResult
            {
                Success = false,
                Message = $"Failed to download repository archive: {ex.Message}"
            };
        }
    }
}

public static class PluginRepositoryArchiveHelper
{
    public static bool TryBuildGitArchiveUrl(string repositoryUrl, string branch, out string archiveUrl, out string error)
    {
        error = string.Empty;
        archiveUrl = string.Empty;

        if (!Uri.TryCreate(repositoryUrl, UriKind.Absolute, out var uri))
        {
            error = "Repository URL must be a valid absolute URL";
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            error = "Only https repository URLs are supported";
            return false;
        }

        if (!repositoryUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            error = "Only GitHub repository URLs are supported in this release";
            return false;
        }

        var path = uri.AbsolutePath.Trim('/');
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            error = "Invalid GitHub repository URL";
            return false;
        }

        var owner = parts[0];
        var repo = parts[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? parts[1][..^4]
            : parts[1];
        var targetBranch = string.IsNullOrWhiteSpace(branch) ? "main" : branch.Trim();
        archiveUrl = $"https://github.com/{owner}/{repo}/archive/refs/heads/{targetBranch}.zip";
        return true;
    }

    public static string? FindManifestPath(string extractRoot, string? repositorySubpath)
    {
        if (!string.IsNullOrWhiteSpace(repositorySubpath))
        {
            var normalized = repositorySubpath.Replace('\\', '/').Trim('/');
            foreach (var directory in Directory.GetDirectories(extractRoot))
            {
                var candidate = Path.Combine(directory, normalized, "polybucket-plugin.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return FindManifestRecursive(extractRoot);
    }

    private static string? FindManifestRecursive(string directory)
    {
        var direct = Path.Combine(directory, "polybucket-plugin.json");
        if (File.Exists(direct))
        {
            return direct;
        }

        foreach (var sub in Directory.GetDirectories(directory))
        {
            var found = FindManifestRecursive(sub);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
