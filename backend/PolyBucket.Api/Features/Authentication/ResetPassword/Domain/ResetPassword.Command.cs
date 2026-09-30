using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Http;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Domain
{
    public class ResetPasswordCommand
    {
        [Required]
        [StringLength(512)]
        public string Token { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }
}
