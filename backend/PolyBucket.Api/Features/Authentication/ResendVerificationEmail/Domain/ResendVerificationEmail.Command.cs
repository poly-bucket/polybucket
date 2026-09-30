using PolyBucket.Api.Common.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Domain
{
    public class ResendVerificationEmailCommand
    {
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }
}
