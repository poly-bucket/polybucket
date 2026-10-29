using PolyBucket.Api.Common.Http;
using PolyBucket.Email;

namespace PolyBucket.Api.Features.Authentication.Logout.Domain;

public class LogoutCommand
{
    public string RefreshToken { get; set; } = string.Empty;
    public ClientRequestInfo Client { get; set; } = new(string.Empty, string.Empty);
}
