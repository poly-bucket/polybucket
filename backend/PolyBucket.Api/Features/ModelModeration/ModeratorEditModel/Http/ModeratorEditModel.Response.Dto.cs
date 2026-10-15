using PolyBucket.Api.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;

[DisplayName("ModeratorEditModelResponseDto")]
public class ModeratorEditModelResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public LicenseTypes? License { get; set; }
    public PrivacySettings Privacy { get; set; }
    public bool AIGenerated { get; set; }
    public bool WIP { get; set; }
    public bool NSFW { get; set; }
    public bool IsRemix { get; set; }
    public string? RemixUrl { get; set; }
    public bool IsPublic { get; set; }
    public bool IsFeatured { get; set; }
    public Guid AuthorId { get; set; }
    public ModeratorEditModelAuthorDto? Author { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<ModeratorEditModelFileDto> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ModeratorEditModelAuthorDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class ModeratorEditModelFileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string MimeType { get; set; } = string.Empty;
}
