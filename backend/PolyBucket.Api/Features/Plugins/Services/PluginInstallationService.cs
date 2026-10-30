using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.Services;

public class PluginInstallationService
{
    private readonly IPluginArchiveDownloader _archiveDownloader;
    private readonly PluginManifestValidator _manifestValidator;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<PluginInstallationService> _logger;
    private readonly string _pluginsPath;

    public PluginInstallationService(
        IPluginArchiveDownloader archiveDownloader,
        PluginManifestValidator manifestValidator,
        IHostApplicationLifetime lifetime,
        ILogger<PluginInstallationService> logger)
    {
        _archiveDownloader = archiveDownloader;
        _manifestValidator = manifestValidator;
        _lifetime = lifetime;
        _logger = logger;
        _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        Directory.CreateDirectory(_pluginsPath);
    }

    public Task<PluginInstallationResult> InstallFromUrlAsync(string url) =>
        InstallFromGitHubAsync(url, null);

    public async Task<PluginInstallationResult> InstallFromGitHubAsync(string repoUrl, string? branch = null)
    {
        return await InstallFromRepositoryAsync(repoUrl, branch ?? "main", null);
    }

    public Task<PluginInstallationResult> InstallFromMarketplaceAsync(MarketplacePluginDetails listing)
    {
        if (string.IsNullOrWhiteSpace(listing.RepositoryUrl))
        {
            return Task.FromResult(new PluginInstallationResult
            {
                Success = false,
                Message = "Marketplace listing is missing a repository URL",
                Errors = { "Marketplace listing is missing a repository URL" }
            });
        }

        return InstallFromRepositoryAsync(
            listing.RepositoryUrl,
            string.IsNullOrWhiteSpace(listing.RepositoryBranch) ? "main" : listing.RepositoryBranch,
            listing.RepositorySubpath);
    }

    public async Task<PluginInstallationResult> InstallFromRepositoryAsync(
        string repositoryUrl,
        string branch,
        string? repositorySubpath)
    {
        try
        {
            var download = await _archiveDownloader.DownloadRepositoryArchiveAsync(repositoryUrl, branch);
            if (!download.Success || download.ArchiveBytes == null)
            {
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = download.Message,
                    Errors = { download.Message }
                };
            }

            return await InstallFromArchiveBytesAsync(download.ArchiveBytes, repositorySubpath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing plugin from repository {RepositoryUrl}", repositoryUrl);
            return new PluginInstallationResult
            {
                Success = false,
                Message = $"Installation failed: {ex.Message}",
                Errors = { ex.Message }
            };
        }
    }

    internal async Task<PluginInstallationResult> InstallFromArchiveBytesAsync(byte[] archiveBytes, string? repositorySubpath)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);

        try
        {
            using (var stream = new MemoryStream(archiveBytes))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(tempPath);
            }

            var manifestPath = PluginRepositoryArchiveHelper.FindManifestPath(tempPath, repositorySubpath);
            if (manifestPath == null)
            {
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = "Plugin manifest not found in repository",
                    Errors = { "Plugin manifest not found in repository" }
                };
            }

            var validationResult = await _manifestValidator.ValidateManifestAsync(manifestPath);
            if (!validationResult.IsValid)
            {
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = validationResult.Message,
                    Errors = { validationResult.Message }
                };
            }

            var manifestContent = await File.ReadAllTextAsync(manifestPath);
            var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (manifest == null)
            {
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = "Failed to parse plugin manifest",
                    Errors = { "Failed to parse plugin manifest" }
                };
            }

            var sourceDir = Path.GetDirectoryName(manifestPath);
            if (sourceDir == null)
            {
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = "Invalid plugin source directory",
                    Errors = { "Invalid plugin source directory" }
                };
            }

            var installationPath = Path.Combine(_pluginsPath, manifest.Id);
            if (Directory.Exists(installationPath))
            {
                Directory.Delete(installationPath, true);
            }

            Directory.CreateDirectory(installationPath);
            CopyDirectory(sourceDir, installationPath);

            var restartRequired = PluginManifestTargets.TargetsApi(manifest.Targets);
            if (restartRequired)
            {
                ScheduleApiRestart();
            }

            _logger.LogInformation(
                "Installed plugin {PluginId} version {Version}. RestartRequired={RestartRequired}",
                manifest.Id,
                manifest.Version,
                restartRequired);

            return new PluginInstallationResult
            {
                Success = true,
                Message = restartRequired
                    ? $"Plugin {manifest.Name} installed. The API will restart to load backend plugin code."
                    : $"Plugin {manifest.Name} installed successfully",
                PluginId = manifest.Id,
                Version = manifest.Version,
                InstallationPath = installationPath,
                RestartRequired = restartRequired
            };
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    public Task<bool> UninstallPluginAsync(string pluginId)
    {
        try
        {
            var pluginPath = Path.Combine(_pluginsPath, pluginId);
            if (Directory.Exists(pluginPath))
            {
                Directory.Delete(pluginPath, true);
                _logger.LogInformation("Successfully uninstalled plugin {PluginId}", pluginId);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Plugin {PluginId} not found for uninstallation", pluginId);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uninstalling plugin {PluginId}", pluginId);
            return Task.FromResult(false);
        }
    }

    public async Task<List<InstalledPlugin>> GetInstalledPluginsAsync()
    {
        var installedPlugins = new List<InstalledPlugin>();

        try
        {
            if (!Directory.Exists(_pluginsPath))
            {
                return installedPlugins;
            }

            foreach (var pluginDir in Directory.GetDirectories(_pluginsPath))
            {
                var manifestPath = Path.Combine(pluginDir, "polybucket-plugin.json");
                if (!File.Exists(manifestPath))
                {
                    continue;
                }

                try
                {
                    var manifestContent = await File.ReadAllTextAsync(manifestPath);
                    var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (manifest == null)
                    {
                        continue;
                    }

                    installedPlugins.Add(new InstalledPlugin
                    {
                        Id = manifest.Id,
                        Name = manifest.Name,
                        Version = manifest.Version,
                        Author = manifest.Author,
                        Description = manifest.Description,
                        Type = manifest.Type,
                        Status = "active",
                        InstalledAt = Directory.GetCreationTimeUtc(pluginDir),
                        UpdatedAt = Directory.GetLastWriteTimeUtc(pluginDir),
                        InstallationPath = pluginDir,
                        Permissions = manifest.Permissions,
                        Targets = PluginManifestTargets.NormalizeTargets(manifest.Targets).ToList(),
                        CssAssets = manifest.Assets?.Css ?? new List<string>(),
                        IsEnabled = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading plugin manifest from {ManifestPath}", manifestPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting installed plugins");
        }

        return installedPlugins;
    }

    public string GetPluginsRootPath() => _pluginsPath;

    public bool TryResolveAssetPath(string pluginId, string relativePath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(pluginId) || string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var pluginRoot = Path.GetFullPath(Path.Combine(_pluginsPath, pluginId));
        if (!pluginRoot.StartsWith(Path.GetFullPath(_pluginsPath), StringComparison.Ordinal))
        {
            return false;
        }

        var normalizedRelative = relativePath.Replace('\\', '/').TrimStart('/');
        if (normalizedRelative.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        var candidate = Path.GetFullPath(Path.Combine(pluginRoot, normalizedRelative));
        if (!candidate.StartsWith(pluginRoot, StringComparison.Ordinal))
        {
            return false;
        }

        if (!File.Exists(candidate))
        {
            return false;
        }

        fullPath = candidate;
        return true;
    }

    private void ScheduleApiRestart()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), _lifetime.ApplicationStopping).ConfigureAwait(false);
                _lifetime.StopApplication();
            }
            catch (OperationCanceledException)
            {
            }
        }, CancellationToken.None);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(directory, Path.Combine(destinationDir, Path.GetFileName(directory)));
        }
    }
}
