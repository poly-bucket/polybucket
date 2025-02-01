using Core.Extensions.Models;
using Core.Models.Users.Settings;

namespace Core.Models.Users
{
    public class User : Auditable
    {
        /// <summary>
        /// A user's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The username of a particular user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The first name of a particular user.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// The last name of a particular user.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// A user's password salt
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// A user's password hash
        /// </summary>
        public string PasswordHash { get; set; }

        public string? Country { get; set; }

        public UserRole Role { get; set; }

        public virtual UserSettings Settings { get; set; }
        public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
    }
}