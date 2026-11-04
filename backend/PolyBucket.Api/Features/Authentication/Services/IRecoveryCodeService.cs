using PolyBucket.Database.Entities.Authentication;

namespace PolyBucket.Api.Features.Authentication.Services;

public sealed record RecoveryCodeGenerationResult(
    IReadOnlyList<string> PlaintextCodes,
    IReadOnlyList<AccountRecoveryCode> Entities);

public interface IRecoveryCodeService
{
    RecoveryCodeGenerationResult CreateBatchEntities(Guid userId, Guid actorUserId, DateTime utcNow);

    bool TryMatchCode(IReadOnlyList<AccountRecoveryCode> unusedCodes, string rawCode, out AccountRecoveryCode? matchedCode);

    string SerializeDisplayPayload(IReadOnlyList<string> plaintextCodes);

    IReadOnlyList<string>? DeserializeDisplayPayload(string? protectedPayload);
}
