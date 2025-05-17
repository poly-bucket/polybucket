namespace Api.Controllers.Authentication.Login.Domain
{
    public class CreateUserLoginRequest
    {
        public string EmailOrUsername { get; set; }
        public string Password { get; set; }
        public string UserAgent { get; set; }
    }
}