using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Http;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Domain
{
    public class RefreshTokenCommand
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }
}
