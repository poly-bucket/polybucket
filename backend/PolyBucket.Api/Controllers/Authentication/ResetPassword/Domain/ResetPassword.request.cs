using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.ResetPassword.Domain
{
    public class ResetPassword
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
}