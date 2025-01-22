namespace Core.Models.Users
{
    public class User : Auditable.Auditable
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
    }
}