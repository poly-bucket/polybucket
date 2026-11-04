using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolyBucket.Api.Common.Http;
using PolyBucket.Email;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Domain;

public class RecoveryCodeResetPasswordCommand
{
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    public string RecoveryCode { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [JsonIgnore]
    public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
}
