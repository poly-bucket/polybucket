using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Authentication.OAuth.Domain
{
    public class OAuthCallbackCommand
    {
        [Required]
        public string Provider { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        public string? State { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public string UserAgent { get; set; } = string.Empty;
    }
} 