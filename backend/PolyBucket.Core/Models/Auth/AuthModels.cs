using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Auth
{
    public class RegisterUserRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsAdmin { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsEmailVerified { get; set; }
        public List<string> Roles { get; set; }
    }
}