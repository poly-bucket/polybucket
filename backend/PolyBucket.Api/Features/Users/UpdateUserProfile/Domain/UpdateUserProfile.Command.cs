using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Domain
{
    public class UpdateUserProfileCommand
    {
        public Guid UserId { get; set; }
        public string? Bio { get; set; }
        public string? Country { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public bool IsProfilePublic { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowLastLogin { get; set; }
        public bool ShowStatistics { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        [StringLength(500)]
        public string? Bio { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? WebsiteUrl { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? TwitterUrl { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? InstagramUrl { get; set; }
        
        [Url]
        [StringLength(500)]
        public string? YouTubeUrl { get; set; }
        
        public bool IsProfilePublic { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowLastLogin { get; set; }
        public bool ShowStatistics { get; set; }
    }
}
