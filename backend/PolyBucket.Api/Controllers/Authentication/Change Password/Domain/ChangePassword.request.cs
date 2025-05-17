using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.Change_Password.Domain
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}