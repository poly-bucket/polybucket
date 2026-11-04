using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Database.Entities.Authentication;

namespace PolyBucket.Api.Features.Authentication.Services;

public sealed class RecoveryCodeService(
    IOptions<RecoveryCodeSettings> settings,
    IRecoveryCodeProtector protector) : IRecoveryCodeService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly RecoveryCodeSettings _settings = settings.Value;

    public RecoveryCodeGenerationResult CreateBatchEntities(Guid userId, Guid actorUserId, DateTime utcNow)
    {
        var plaintextCodes = new List<string>(_settings.CodeCount);
        var entities = new List<AccountRecoveryCode>(_settings.CodeCount);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (plaintextCodes.Count < _settings.CodeCount)
        {
            var code = GenerateCode();
            if (!seen.Add(code))
            {
                continue;
            }

            plaintextCodes.Add(code);
            entities.Add(new AccountRecoveryCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProtectedCodeHash = protector.ProtectHash(TokenHasher.Hash(RecoveryCodeNormalizer.Normalize(code))),
                IsUsed = false,
                CreatedAt = utcNow,
                CreatedById = actorUserId,
                UpdatedAt = utcNow,
                UpdatedById = actorUserId,
                Version = 1
            });
        }

        return new RecoveryCodeGenerationResult(plaintextCodes, entities);
    }

    public bool TryMatchCode(IReadOnlyList<AccountRecoveryCode> unusedCodes, string rawCode, out AccountRecoveryCode? matchedCode)
    {
        matchedCode = null;
        if (unusedCodes.Count == 0 || string.IsNullOrWhiteSpace(rawCode))
        {
            return false;
        }

        string normalized;
        try
        {
            normalized = RecoveryCodeNormalizer.Normalize(rawCode);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var candidateHash = TokenHasher.Hash(normalized);
        var candidateBytes = Encoding.UTF8.GetBytes(candidateHash);

        foreach (var code in unusedCodes)
        {
            string storedHash;
            try
            {
                storedHash = protector.UnprotectHash(code.ProtectedCodeHash);
            }
            catch (CryptographicException)
            {
                continue;
            }

            var storedBytes = Encoding.UTF8.GetBytes(storedHash);
            if (storedBytes.Length == candidateBytes.Length
                && CryptographicOperations.FixedTimeEquals(storedBytes, candidateBytes))
            {
                matchedCode = code;
                return true;
            }
        }

        return false;
    }

    public string SerializeDisplayPayload(IReadOnlyList<string> plaintextCodes)
    {
        var json = JsonSerializer.Serialize(plaintextCodes);
        return protector.ProtectDisplayPayload(json);
    }

    public IReadOnlyList<string>? DeserializeDisplayPayload(string? protectedPayload)
    {
        var json = protector.UnprotectDisplayPayload(protectedPayload);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<string>>(json);
    }

    private string GenerateCode()
    {
        Span<char> buffer = stackalloc char[_settings.CodeLength];
        Span<byte> randomBytes = stackalloc byte[_settings.CodeLength];
        RandomNumberGenerator.Fill(randomBytes);
        for (var i = 0; i < _settings.CodeLength; i++)
        {
            buffer[i] = CodeAlphabet[randomBytes[i] % CodeAlphabet.Length];
        }

        return new string(buffer);
    }
}
