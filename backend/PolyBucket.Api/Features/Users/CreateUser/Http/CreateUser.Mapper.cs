using PolyBucket.Api.Features.Users.CreateUser.Domain;

namespace PolyBucket.Api.Features.Users.CreateUser.Http;

public static class CreateUserResponseMapper
{
    public static CreateUserResponseDto ToDto(CreateUserCommandResponse response) => new()
    {
        UserId = response.UserId,
        Email = response.Email,
        Username = response.Username,
        RoleId = response.RoleId,
        RoleName = response.RoleName,
        FirstName = response.FirstName,
        LastName = response.LastName,
        Country = response.Country,
        GeneratedPassword = response.GeneratedPassword,
        InviteEmailQueued = response.InviteEmailQueued,
        CreatedAt = response.CreatedAt,
        EmailVerificationRequired = response.EmailVerificationRequired
    };
}
