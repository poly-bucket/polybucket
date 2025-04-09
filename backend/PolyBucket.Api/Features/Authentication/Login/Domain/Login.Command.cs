namespace PolyBucket.Api.Features.Authentication.Login.Domain
{
    public class LoginCommand
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
} 