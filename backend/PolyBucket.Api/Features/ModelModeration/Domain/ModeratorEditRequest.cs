using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ModelModeration.Domain
{
    public class ModeratorEditRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        public LicenseTypes? License { get; set; }
        
        public PrivacySettings Privacy { get; set; }
        
        public bool AIGenerated { get; set; }
        
        public bool WIP { get; set; }
        
        public bool NSFW { get; set; }
        
        public bool IsRemix { get; set; }
        
        [Url]
        public string? RemixUrl { get; set; }
        
        public bool IsPublic { get; set; }
        
        public bool IsFeatured { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public List<string> Categories { get; set; } = new List<string>();
        
        [StringLength(1000)]
        public string? ModerationNotes { get; set; }
        
        [Required]
        public ModerationAction Action { get; set; }
    }

    public enum ModerationAction
    {
        Edit,
        ApproveWithChanges,
        FlagForReview,
        FeatureModel,
        UnfeatureModel
    }
} 