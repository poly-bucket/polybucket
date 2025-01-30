namespace Core.Models.Users
{
    public class UserLogin : BaseEntity
    {
        public string Email { get; set; }

        public bool? Successful { get; set; }

        public string? IpAddress { get; set; }

        public string UserAgent { get; set; }
    }
}