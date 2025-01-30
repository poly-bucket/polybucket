namespace Api.Controllers.Authentication.Domain
{
    public class CreateUserLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserAgent { get; set; }
    }
}