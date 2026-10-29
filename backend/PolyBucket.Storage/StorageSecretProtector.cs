using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace PolyBucket.Storage;

public interface IStorageSecretProtector
{
    string Protect(string plaintext);
    StorageSecretReadResult Unprotect(string? stored);
}

public readonly record struct StorageSecretReadResult(string Plaintext, bool IsLegacyPlaintext, bool IsUnreadable);

public class StorageSecretProtector(IDataProtectionProvider dataProtectionProvider) : IStorageSecretProtector
{
    public const string Purpose = "PolyBucket.Storage.SecretKey";
    public const string Prefix = "enc:v1:";

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(Purpose);

    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return string.Empty;
        }

        return Prefix + _protector.Protect(plaintext);
    }

    public StorageSecretReadResult Unprotect(string? stored)
    {
        if (string.IsNullOrEmpty(stored))
        {
            return new StorageSecretReadResult(string.Empty, false, false);
        }

        if (!stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return new StorageSecretReadResult(stored, true, false);
        }

        try
        {
            return new StorageSecretReadResult(_protector.Unprotect(stored[Prefix.Length..]), false, false);
        }
        catch (CryptographicException)
        {
            return new StorageSecretReadResult(string.Empty, false, true);
        }
    }
}
