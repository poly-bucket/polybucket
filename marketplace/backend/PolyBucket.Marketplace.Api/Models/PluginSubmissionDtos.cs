using System.Text.Json.Serialization;

namespace PolyBucket.Marketplace.Api.Models;

public class RemotePluginManifest
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

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("targets")]
    public List<string> Targets { get; set; } = new();

    [JsonPropertyName("license")]
    public string License { get; set; } = string.Empty;
}

public class RemoteManifestReadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public RemotePluginManifest? Manifest { get; set; }
}

public class CreateSubmissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string RepositoryBranch { get; set; } = "main";
    public string RepositorySubpath { get; set; } = string.Empty;
}

public class RejectSubmissionRequest
{
    public string ReviewNotes { get; set; } = string.Empty;
}

public class SubmissionSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string RepositoryBranch { get; set; } = string.Empty;
    public string RepositorySubpath { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class ApproveSubmissionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PluginId { get; set; }
}
