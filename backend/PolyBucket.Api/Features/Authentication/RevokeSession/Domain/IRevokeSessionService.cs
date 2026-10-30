namespace PolyBucket.Api.Features.Authentication.RevokeSession.Domain;

public interface IRevokeSessionService
{
    Task<RevokeSessionResult> RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
}

public class RevokeSessionResult
{
    public bool Found { get; init; }
    public static RevokeSessionResult Success() => new() { Found = true };
    public static RevokeSessionResult NotFound() => new() { Found = false };
}
