namespace PolyBucket.Api.Features.Users.CreateUser.Domain
{
    public class CreateUserCommandResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }
        public string GeneratedPassword { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool EmailVerificationRequired { get; set; }
    }
} 