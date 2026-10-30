using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.Services;

public class PluginInstallationServiceTests : IDisposable
{
    private readonly string _pluginsRoot;
    private readonly PluginInstallationService _service;

    public PluginInstallationServiceTests()
    {
        _pluginsRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_pluginsRoot);

        var lifetime = new TestHostApplicationLifetime();
        _service = new PluginInstallationService(
            new FakeArchiveDownloader(CreateDarkThemeArchiveBytes()),
            new PluginManifestValidator(NullLogger<PluginManifestValidator>.Instance),
            lifetime,
            NullLogger<PluginInstallationService>.Instance);

        typeof(PluginInstallationService)
            .GetField("_pluginsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(_service, _pluginsRoot);
    }

    [Fact(DisplayName = "When a frontend-only plugin installs, RestartRequired is false and CSS is copied.")]
    public async Task InstallFromArchive_FrontendPlugin_SetsRestartRequiredFalse()
    {
        // Arrange
        var archive = CreateDarkThemeArchiveBytes();

        // Act
        var result = await _service.InstallFromArchiveBytesAsync(archive, null);

        // Assert
        result.Success.ShouldBeTrue();
        result.RestartRequired.ShouldBeFalse();
        File.Exists(Path.Combine(_pluginsRoot, "dark-theme-plugin", "styles", "dark-theme.css")).ShouldBeTrue();
    }

    [Fact(DisplayName = "When an API-target plugin installs, RestartRequired is true.")]
    public async Task InstallFromArchive_ApiPlugin_SetsRestartRequiredTrue()
    {
        // Arrange
        var archive = CreateManifestArchive(
            "sample-api-plugin",
            "Sample API Plugin",
            new[] { "api" },
            Array.Empty<string>());

        // Act
        var result = await _service.InstallFromArchiveBytesAsync(archive, null);

        // Assert
        result.Success.ShouldBeTrue();
        result.RestartRequired.ShouldBeTrue();
    }

    [Fact(DisplayName = "When asset path escapes the plugin directory, TryResolveAssetPath returns false.")]
    public void TryResolveAssetPath_RejectsPathTraversal()
    {
        // Arrange
        var pluginDir = Path.Combine(_pluginsRoot, "dark-theme-plugin");
        Directory.CreateDirectory(pluginDir);
        File.WriteAllText(Path.Combine(pluginDir, "polybucket-plugin.json"), "{}");

        // Act
        var allowed = _service.TryResolveAssetPath("dark-theme-plugin", "../secrets.txt", out _);

        // Assert
        allowed.ShouldBeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_pluginsRoot))
        {
            Directory.Delete(_pluginsRoot, true);
        }
    }

    private static byte[] CreateDarkThemeArchiveBytes() =>
        CreateManifestArchive(
            "dark-theme-plugin",
            "Dark Theme",
            new[] { "frontend" },
            new[] { "styles/dark-theme.css" });

    private static byte[] CreateManifestArchive(
        string id,
        string name,
        string[] targets,
        string[] cssAssets)
    {
        var manifest = $$"""
        {
          "id": "{{id}}",
          "name": "{{name}}",
          "version": "1.0.0",
          "author": "Test",
          "description": "Test plugin",
          "type": "theme",
          "targets": [{{string.Join(",", targets.Select(t => $"\"{t}\""))}}],
          "assets": { "css": [{{string.Join(",", cssAssets.Select(c => $"\"{c}\""))}}], "js": [], "images": [] }
        }
        """;

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("repo-main/polybucket-plugin.json");
            using (var writer = new StreamWriter(entry.Open(), Encoding.UTF8))
            {
                writer.Write(manifest);
            }

            foreach (var css in cssAssets)
            {
                var cssEntry = archive.CreateEntry($"repo-main/{css}");
                using (var cssWriter = new StreamWriter(cssEntry.Open(), Encoding.UTF8))
                {
                    cssWriter.Write(".test{}");
                }
            }
        }

        return stream.ToArray();
    }

    private sealed class FakeArchiveDownloader(byte[] bytes) : IPluginArchiveDownloader
    {
        public Task<PluginArchiveDownloadResult> DownloadRepositoryArchiveAsync(
            string repositoryUrl,
            string branch,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PluginArchiveDownloadResult { Success = true, ArchiveBytes = bytes });
    }

    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted { get; } = CancellationToken.None;
        public CancellationToken ApplicationStopping { get; } = CancellationToken.None;
        public CancellationToken ApplicationStopped { get; } = CancellationToken.None;
        public void StopApplication()
        {
        }
    }
}
