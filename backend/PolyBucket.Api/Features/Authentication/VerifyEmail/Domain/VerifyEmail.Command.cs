using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Domain
{
    public class VerifyEmailCommand
    {
        [Required]
        [StringLength(512)]
        public string Token { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }
    }
}
