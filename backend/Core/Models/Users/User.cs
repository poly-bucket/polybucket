using Core.Extensions.Models;

namespace Core.Models.Users
{
    public class User : Auditable
    {
        public Guid Id { get; set; }

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
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of a particular user.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// A user's password salt
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// A user's password hash
        /// </summary>
        public string PasswordHash { get; set; }
    }
}