namespace Core.Models.Users
{
    public class UserLogin
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string IpAddress { get; set; }

        public string UserAgent { get; set; }
    }
}