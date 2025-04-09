using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Domain
{
    public class RefreshTokenCommand
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
} 