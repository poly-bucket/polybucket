using PolyBucket.Api.Common.Http;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain
{
    public class EnableTwoFactorAuthCommand
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }

    public class EnableTwoFactorAuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public IEnumerable<string>? BackupCodes { get; set; }
    }
}
