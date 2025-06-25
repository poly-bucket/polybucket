namespace PolyBucket.Api.Features.Authentication.Commands
{
    public class LoginCommand
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserAgent { get; set; }
    }

    public class LoginCommandResponse
    {
        public string Token { get; set; }
    }
} 