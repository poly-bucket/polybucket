using System;

namespace PolyBucket.Api.Features.Users.GetUserById.Domain;

public class GetUserByIdResult
{
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Username { get; set; } = null!;
}
