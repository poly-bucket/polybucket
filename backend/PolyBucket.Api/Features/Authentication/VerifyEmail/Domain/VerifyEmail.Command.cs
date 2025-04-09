using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Domain
{
    public class VerifyEmailCommand
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
} 