using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.ForgotPassword.Domain
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}