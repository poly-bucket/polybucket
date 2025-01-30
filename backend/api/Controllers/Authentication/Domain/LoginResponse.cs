public class LoginResponse
{
    public string Token { get; set; }
    public UserResponse User { get; set; }
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
} 