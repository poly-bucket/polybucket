using PolyBucket.Api.Common.Http;
using System;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain
{
    public class DisableTwoFactorAuthCommand
    {
        public Guid UserId { get; set; }

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }

    public class DisableTwoFactorAuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }
}
