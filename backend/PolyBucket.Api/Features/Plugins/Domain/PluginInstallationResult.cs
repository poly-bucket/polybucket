using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Plugins.Domain
{
    public class PluginInstallationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
        public string InstallationPath { get; set; } = string.Empty;
    }

    public class PluginInstallationRequest
    {
        public string Source { get; set; } = string.Empty; // "github", "url", "marketplace"
        public string Url { get; set; } = string.Empty;
        public string? Branch { get; set; }
        public string? PluginId { get; set; }
        public string? Version { get; set; }
    }

    public class InstalledPlugin
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "active", "inactive", "error"
        public DateTime InstalledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string InstallationPath { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public bool IsEnabled { get; set; }
    }

    public class MarketplacePlugin
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public int DownloadCount { get; set; }
        public double Rating { get; set; }
        public string RepositoryUrl { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public bool IsInstalled { get; set; }
    }
}
