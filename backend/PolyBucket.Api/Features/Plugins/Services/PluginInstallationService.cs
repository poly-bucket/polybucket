using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.Services
{
    public class PluginInstallationService
    {
        private readonly HttpClient _httpClient;
        private readonly PluginManifestValidator _manifestValidator;
        private readonly ILogger<PluginInstallationService> _logger;
        private readonly string _pluginsPath;

        public PluginInstallationService(
            HttpClient httpClient,
            PluginManifestValidator manifestValidator,
            ILogger<PluginInstallationService> logger)
        {
            _httpClient = httpClient;
            _manifestValidator = manifestValidator;
            _logger = logger;
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
            
            // Ensure plugins directory exists
            Directory.CreateDirectory(_pluginsPath);
        }

        public async Task<PluginInstallationResult> InstallFromUrlAsync(string url)
        {
            try
            {
                _logger.LogInformation("Installing plugin from URL: {Url}", url);

                // Download plugin package
                var pluginData = await _httpClient.GetByteArrayAsync(url);
                
                // Extract to temporary directory
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);
                
                using (var stream = new MemoryStream(pluginData))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(tempPath);
                }

                // Validate manifest
                var manifestPath = Path.Combine(tempPath, "polybucket-plugin.json");
                var validationResult = await _manifestValidator.ValidateManifestAsync(manifestPath);
                
                if (!validationResult.IsValid)
                {
                    Directory.Delete(tempPath, true);
                    return new PluginInstallationResult
                    {
                        Success = false,
                        Message = validationResult.Message,
                        Errors = new() { validationResult.Message }
                    };
                }

                // Read manifest
                var manifestContent = await File.ReadAllTextAsync(manifestPath);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest == null)
                {
                    Directory.Delete(tempPath, true);
                    return new PluginInstallationResult
                    {
                        Success = false,
                        Message = "Failed to parse plugin manifest",
                        Errors = new() { "Failed to parse plugin manifest" }
                    };
                }

                // Install plugin
                var installationPath = Path.Combine(_pluginsPath, manifest.Id);
                if (Directory.Exists(installationPath))
                {
                    Directory.Delete(installationPath, true);
                }

                Directory.Move(tempPath, installationPath);

                _logger.LogInformation("Successfully installed plugin {PluginId} version {Version}", 
                    manifest.Id, manifest.Version);

                return new PluginInstallationResult
                {
                    Success = true,
                    Message = $"Plugin {manifest.Name} installed successfully",
                    PluginId = manifest.Id,
                    Version = manifest.Version,
                    InstallationPath = installationPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing plugin from URL: {Url}", url);
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = $"Installation failed: {ex.Message}",
                    Errors = new() { ex.Message }
                };
            }
        }

        public async Task<PluginInstallationResult> InstallFromGitHubAsync(string repoUrl, string? branch = null)
        {
            try
            {
                _logger.LogInformation("Installing plugin from GitHub: {RepoUrl}, branch: {Branch}", repoUrl, branch ?? "main");

                // Parse GitHub URL to get owner and repo
                var urlParts = repoUrl.Replace("https://github.com/", "").Split('/');
                if (urlParts.Length < 2)
                {
                    return new PluginInstallationResult
                    {
                        Success = false,
                        Message = "Invalid GitHub repository URL",
                        Errors = new() { "Invalid GitHub repository URL" }
                    };
                }

                var owner = urlParts[0];
                var repo = urlParts[1];
                var targetBranch = branch ?? "main";

                // Download repository as ZIP
                var downloadUrl = $"https://github.com/{owner}/{repo}/archive/refs/heads/{targetBranch}.zip";
                var pluginData = await _httpClient.GetByteArrayAsync(downloadUrl);

                // Extract to temporary directory
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);
                
                using (var stream = new MemoryStream(pluginData))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(tempPath);
                }

                // Find the manifest file (it might be in a subdirectory)
                var manifestPath = FindManifestFile(tempPath);
                if (manifestPath == null)
                {
                    Directory.Delete(tempPath, true);
                    return new PluginInstallationResult
                    {
                        Success = false,
                        Message = "Plugin manifest not found in repository",
                        Errors = new() { "Plugin manifest not found in repository" }
                    };
                }

                // Validate manifest
                var validationResult = await _manifestValidator.ValidateManifestAsync(manifestPath);
                
                if (!validationResult.IsValid)
                {
                    Directory.Delete(tempPath, true);
                    return new PluginInstallationResult
                    {
                        Success = false,
                        Message = validationResult.Message,
                        Errors = new() { validationResult.Message }
                    };
                }

                // Read manifest
                var manifestContent = await File.ReadAllTextAsync(manifestPath);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest == null)
                {
                    Directory.Delete(tempPath, true);
                    return new PluginInstallationResult
                    {
                        Success = false,
                        Message = "Failed to parse plugin manifest",
                        Errors = new() { "Failed to parse plugin manifest" }
                    };
                }

                // Install plugin
                var installationPath = Path.Combine(_pluginsPath, manifest.Id);
                if (Directory.Exists(installationPath))
                {
                    Directory.Delete(installationPath, true);
                }

                // Move the plugin files to the installation directory
                var sourceDir = Path.GetDirectoryName(manifestPath);
                if (sourceDir != null)
                {
                    Directory.Move(sourceDir, installationPath);
                }

                _logger.LogInformation("Successfully installed plugin {PluginId} version {Version} from GitHub", 
                    manifest.Id, manifest.Version);

                return new PluginInstallationResult
                {
                    Success = true,
                    Message = $"Plugin {manifest.Name} installed successfully from GitHub",
                    PluginId = manifest.Id,
                    Version = manifest.Version,
                    InstallationPath = installationPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing plugin from GitHub: {RepoUrl}", repoUrl);
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = $"Installation failed: {ex.Message}",
                    Errors = new() { ex.Message }
                };
            }
        }

        public async Task<PluginInstallationResult> InstallFromMarketplaceAsync(string pluginId, string version)
        {
            // TODO: Implement marketplace installation
            // This would involve fetching plugin metadata from marketplace API
            // and downloading the plugin package
            await Task.CompletedTask;
            
            return new PluginInstallationResult
            {
                Success = false,
                Message = "Marketplace installation not yet implemented",
                Errors = new() { "Marketplace installation not yet implemented" }
            };
        }

        public async Task<bool> UninstallPluginAsync(string pluginId)
        {
            try
            {
                var pluginPath = Path.Combine(_pluginsPath, pluginId);
                if (Directory.Exists(pluginPath))
                {
                    Directory.Delete(pluginPath, true);
                    _logger.LogInformation("Successfully uninstalled plugin {PluginId}", pluginId);
                    return true;
                }
                
                _logger.LogWarning("Plugin {PluginId} not found for uninstallation", pluginId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uninstalling plugin {PluginId}", pluginId);
                return false;
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

                var pluginDirectories = Directory.GetDirectories(_pluginsPath);
                
                foreach (var pluginDir in pluginDirectories)
                {
                    var manifestPath = Path.Combine(pluginDir, "polybucket-plugin.json");
                    if (File.Exists(manifestPath))
                    {
                        try
                        {
                            var manifestContent = await File.ReadAllTextAsync(manifestPath);
                            var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (manifest != null)
                            {
                                var installedPlugin = new InstalledPlugin
                                {
                                    Id = manifest.Id,
                                    Name = manifest.Name,
                                    Version = manifest.Version,
                                    Author = manifest.Author,
                                    Description = manifest.Description,
                                    Type = manifest.Type,
                                    Status = "active", // TODO: Determine actual status
                                    InstalledAt = Directory.GetCreationTime(pluginDir),
                                    UpdatedAt = Directory.GetLastWriteTime(pluginDir),
                                    InstallationPath = pluginDir,
                                    Permissions = manifest.Permissions,
                                    IsEnabled = true // TODO: Check actual enabled status
                                };

                                installedPlugins.Add(installedPlugin);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error reading plugin manifest from {ManifestPath}", manifestPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting installed plugins");
            }

            return installedPlugins;
        }

        private string? FindManifestFile(string directory)
        {
            var manifestPath = Path.Combine(directory, "polybucket-plugin.json");
            if (File.Exists(manifestPath))
            {
                return manifestPath;
            }

            // Search in subdirectories
            var subdirectories = Directory.GetDirectories(directory);
            foreach (var subdir in subdirectories)
            {
                var result = FindManifestFile(subdir);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
