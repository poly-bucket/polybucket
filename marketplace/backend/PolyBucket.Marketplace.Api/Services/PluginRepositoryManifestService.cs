using System.IO.Compression;
using System.Text.Json;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services;

public class PluginRepositoryManifestService(
    HttpClient httpClient,
    ILogger<PluginRepositoryManifestService> logger) : IPluginRepositoryManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<RemoteManifestReadResult> ReadManifestFromRepositoryAsync(
        string repositoryUrl,
        string branch,
        string? repositorySubpath,
        CancellationToken cancellationToken = default)
    {
        if (!TryBuildGitArchiveUrl(repositoryUrl, branch, out var archiveUrl, out var error))
        {
            return new RemoteManifestReadResult { Success = false, Message = error };
        }

        try
        {
            var archiveBytes = await httpClient.GetByteArrayAsync(archiveUrl, cancellationToken);
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);

            try
            {
                using (var stream = new MemoryStream(archiveBytes))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(tempRoot);
                }

                var manifestPath = FindManifestPath(tempRoot, repositorySubpath);
                if (manifestPath == null)
                {
                    return new RemoteManifestReadResult
                    {
                        Success = false,
                        Message = "polybucket-plugin.json was not found in the repository archive"
                    };
                }

                var content = await File.ReadAllTextAsync(manifestPath, cancellationToken);
                var manifest = JsonSerializer.Deserialize<RemotePluginManifest>(content, JsonOptions);
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.Id))
                {
                    return new RemoteManifestReadResult
                    {
                        Success = false,
                        Message = "Failed to parse plugin manifest"
                    };
                }

                if (manifest.Targets == null || manifest.Targets.Count == 0)
                {
                    return new RemoteManifestReadResult
                    {
                        Success = false,
                        Message = "Plugin manifest must declare at least one target (frontend and/or api)"
                    };
                }

                return new RemoteManifestReadResult { Success = true, Manifest = manifest };
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read manifest from repository {RepositoryUrl}", repositoryUrl);
            return new RemoteManifestReadResult
            {
                Success = false,
                Message = $"Failed to read manifest from repository: {ex.Message}"
            };
        }
    }

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

    internal static string? FindManifestPath(string extractRoot, string? repositorySubpath)
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
