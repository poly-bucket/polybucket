using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Authentication.Http
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? UserAgent { get; set; }
    }
} 