using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace PolyBucket.Api.Features.Authentication.Services;

public sealed class RecoveryCodeProtector(IDataProtectionProvider dataProtectionProvider) : IRecoveryCodeProtector
{
    public const string HashPurpose = "PolyBucket.AccountRecoveryCode.Hash";
    public const string DisplayPurpose = "PolyBucket.AccountRecoveryCode.Display";
    public const string Prefix = "enc:v1:";

    private readonly IDataProtector _hashProtector = dataProtectionProvider.CreateProtector(HashPurpose);
    private readonly IDataProtector _displayProtector = dataProtectionProvider.CreateProtector(DisplayPurpose);

    public string ProtectHash(string codeHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(codeHash);
        return Prefix + _hashProtector.Protect(codeHash);
    }

    public string UnprotectHash(string protectedHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(protectedHash);
        if (!protectedHash.StartsWith(Prefix, StringComparison.Ordinal))
        {
            throw new CryptographicException("Recovery code hash is not in the expected protected format.");
        }

        return _hashProtector.Unprotect(protectedHash[Prefix.Length..]);
    }

    public string ProtectDisplayPayload(string jsonPayload)
    {
        ArgumentException.ThrowIfNullOrEmpty(jsonPayload);
        return Prefix + _displayProtector.Protect(jsonPayload);
    }

    public string? UnprotectDisplayPayload(string? protectedPayload)
    {
        if (string.IsNullOrEmpty(protectedPayload))
        {
            return null;
        }

        if (!protectedPayload.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            return _displayProtector.Unprotect(protectedPayload[Prefix.Length..]);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
