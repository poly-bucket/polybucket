using Core.Entities;
using Core.Extensions.Models;

namespace Core.Models.Users
{
    public class UserLogin : BaseEntity
    {
        public string Email { get; set; }

        public bool? Successful { get; set; }

        public string? IpAddress { get; set; }

        public string UserAgent { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}