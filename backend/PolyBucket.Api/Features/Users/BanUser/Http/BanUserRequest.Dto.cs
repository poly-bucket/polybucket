using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Users.BanUser.Http;

public class BanUserRequest
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}
