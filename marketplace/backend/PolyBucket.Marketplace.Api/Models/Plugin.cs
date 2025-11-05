using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Marketplace.Api.Models
{
    public class Plugin
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string Status { get; set; } = "draft"; // draft, pending, published, rejected
        public bool IsVerified { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int Downloads { get; set; }
        public double AverageRating { get; set; }
        public decimal Revenue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? AuthorId { get; set; }

        // Navigation properties
        public MarketplaceUser? Author { get; set; }
        public List<PluginVersion> Versions { get; set; } = new();
        public List<PluginReview> Reviews { get; set; } = new();
    }

    public class PluginVersion
    {
        public string Id { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Plugin Plugin { get; set; } = null!;
    }

    public class PluginCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class PluginReview
    {
        public string Id { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsVerified { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Plugin Plugin { get; set; } = null!;
        public List<ReviewHelpful> Helpfuls { get; set; } = new();
    }

    public class PluginRating
    {
        public string Id { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Plugin Plugin { get; set; } = null!;
        public MarketplaceUser? User { get; set; }
    }

    public class PluginDownload
    {
        public string Id { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? InstanceId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public DateTime DownloadedAt { get; set; }

        // Navigation properties
        public Plugin Plugin { get; set; } = null!;
    }

    public class PluginSubmission
    {
        public string Id { get; set; } = string.Empty;
        public string? SubmitterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // pending, approved, rejected, published
        public string? ReviewNotes { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        public MarketplaceUser? Submitter { get; set; }
    }

    public class ReviewHelpful
    {
        public string Id { get; set; } = string.Empty;
        public string ReviewId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public PluginReview Review { get; set; } = null!;
    }

}
