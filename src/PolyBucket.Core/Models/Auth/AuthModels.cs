using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Core.Models.Auth
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
    
    public class LoginRequest
    {
        [Required]
        public string EmailOrUsername { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
    
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; }
        public UserDto User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
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
    
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
    
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
    
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 