using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Users.CreateUser.Domain
{
    public class CreateUserCommand
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public Guid RoleId { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        // These will be set internally by the system
        public string UserAgent { get; set; } = string.Empty;
        public string CreatedByIp { get; set; } = string.Empty;
    }
} 