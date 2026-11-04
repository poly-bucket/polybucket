namespace PolyBucket.Api.Features.Authentication.Services;

public interface IRecoveryCodeProtector
{
    string ProtectHash(string codeHash);

    string UnprotectHash(string protectedHash);

    string ProtectDisplayPayload(string jsonPayload);

    string? UnprotectDisplayPayload(string? protectedPayload);
}
