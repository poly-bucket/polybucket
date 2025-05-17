using Core.Entities;
using Core.Models.Roles;
using Core.Models.Users.Settings;
using Core.Models;
using Core.Models.Collections;
using Core.Models.Enumerations;
using Core.Models.Models;

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
        public string PasswordSalt { get; set; }

        /// <summary>
        /// A user's password hash
        /// </summary>
        public string PasswordHash { get; set; }

        public int AccessFailedCount { get; set; } = 0;
        public bool IsLocked { get; set; } = false;

        public DateTime? LockoutEnd { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string EmailVerificationToken { get; set; }

        public DateTime? EmailVerificationTokenExpiry { get; set; }

        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        public List<string> Roles { get; set; } = new List<string>();
        public List<string> OrganizationIds { get; set; } = new List<string>();
        public string ProfilePictureUrl { get; set; }

        public UserRole Role { get; set; }

        #region Navigation Properties

        public virtual UserSettings Settings { get; set; }
        public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();

        public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

        public virtual ICollection<Model> Models { get; set; } = new List<Model>();
        public virtual ICollection<RefreshToken.RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken.RefreshToken>();
        public virtual ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
        public virtual ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();

        #endregion Navigation Properties
    }
}