using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Domain
{
    public class ForgotPasswordCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
} 