using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyBucket.Contracts
{
    public class MarketplacePlugin
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("downloads")]
        public int Downloads { get; set; }

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("reviewCount")]
        public int ReviewCount { get; set; }

        [JsonPropertyName("repositoryUrl")]
        public string RepositoryUrl { get; set; } = string.Empty;

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; } = string.Empty;

        [JsonPropertyName("screenshots")]
        public List<string> Screenshots { get; set; } = new();

        [JsonPropertyName("documentationUrl")]
        public string DocumentationUrl { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class MarketplacePluginDetails : MarketplacePlugin
    {
        [JsonPropertyName("readme")]
        public string Readme { get; set; } = string.Empty;

        [JsonPropertyName("changelog")]
        public string Changelog { get; set; } = string.Empty;

        [JsonPropertyName("compatibility")]
        public PluginCompatibility Compatibility { get; set; } = new();

        [JsonPropertyName("dependencies")]
        public List<string> Dependencies { get; set; } = new();

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new();

        [JsonPropertyName("settings")]
        public List<PluginSetting> Settings { get; set; } = new();

        [JsonPropertyName("reviews")]
        public List<PluginReview> Reviews { get; set; } = new();

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;

        [JsonPropertyName("installUrl")]
        public string InstallUrl { get; set; } = string.Empty;
    }

    public class PluginCompatibility
    {
        [JsonPropertyName("minVersion")]
        public string MinVersion { get; set; } = "1.0.0";

        [JsonPropertyName("maxVersion")]
        public string MaxVersion { get; set; } = "2.0.0";

        [JsonPropertyName("testedVersions")]
        public List<string> TestedVersions { get; set; } = new();
    }

    public class PluginSetting
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("defaultValue")]
        public object? DefaultValue { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }

    public class PluginReview
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("userAvatar")]
        public string UserAvatar { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class PluginInstallationRequest
    {
        [JsonPropertyName("pluginId")]
        public string PluginId { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = "marketplace";

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; } = string.Empty;
    }

    public class PluginInstallationResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("pluginId")]
        public string PluginId { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();

        [JsonPropertyName("installedAt")]
        public DateTime InstalledAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class PluginRating
    {
        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
    }

    public class PluginSubmission
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("repositoryUrl")]
        public string RepositoryUrl { get; set; } = string.Empty;

        [JsonPropertyName("license")]
        public string License { get; set; } = string.Empty;

        [JsonPropertyName("authorName")]
        public string AuthorName { get; set; } = string.Empty;

        [JsonPropertyName("authorEmail")]
        public string AuthorEmail { get; set; } = string.Empty;

        [JsonPropertyName("screenshots")]
        public List<string> Screenshots { get; set; } = new();

        [JsonPropertyName("documentationUrl")]
        public string DocumentationUrl { get; set; } = string.Empty;
    }

    public class SubmissionResult
    {
        [JsonPropertyName("submissionId")]
        public string SubmissionId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

    public class SubmissionStatus
    {
        [JsonPropertyName("submissionId")]
        public string SubmissionId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("progress")]
        public int Progress { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("reviewedAt")]
        public DateTime? ReviewedAt { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTime? PublishedAt { get; set; }
    }
}
