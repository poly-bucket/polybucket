using PolyBucket.Api.Common.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Authentication.RequestEmailChange.Domain
{
    public class RequestEmailChangeCommand
    {
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string NewEmail { get; set; } = string.Empty;

        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [JsonIgnore]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }
}
