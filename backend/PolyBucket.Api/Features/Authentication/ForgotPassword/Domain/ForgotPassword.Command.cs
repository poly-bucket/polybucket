using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Http;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Domain
{
    public class ForgotPasswordCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }
}
