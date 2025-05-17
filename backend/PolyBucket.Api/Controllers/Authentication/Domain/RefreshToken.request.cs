using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.Domain;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; }
}