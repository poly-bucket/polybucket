using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using PolyBucket.Api.Common.Http;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Domain
{
    public class ChangePasswordCommand : IRequest<ChangePasswordResponse>
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [JsonIgnore]
        public ClientRequestInfo Client { get; set; } = ClientRequestInfo.Unknown;
    }

    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresFirstTimeSetup { get; set; } = false;
    }
} 