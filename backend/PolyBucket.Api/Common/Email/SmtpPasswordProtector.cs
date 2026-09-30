using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace PolyBucket.Api.Common.Email;

public interface ISmtpPasswordProtector
{
    string Protect(string plaintext);
    SmtpPasswordReadResult Unprotect(string? stored);
}

public readonly record struct SmtpPasswordReadResult(string Plaintext, bool IsLegacyPlaintext, bool IsUnreadable);

public class SmtpPasswordProtector(IDataProtectionProvider dataProtectionProvider) : ISmtpPasswordProtector
{
    public const string Purpose = "PolyBucket.Email.SmtpPassword";
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

    public SmtpPasswordReadResult Unprotect(string? stored)
    {
        if (string.IsNullOrEmpty(stored))
        {
            return new SmtpPasswordReadResult(string.Empty, false, false);
        }

        if (!stored.StartsWith(Prefix, System.StringComparison.Ordinal))
        {
            return new SmtpPasswordReadResult(stored, true, false);
        }

        try
        {
            return new SmtpPasswordReadResult(_protector.Unprotect(stored[Prefix.Length..]), false, false);
        }
        catch (CryptographicException)
        {
            return new SmtpPasswordReadResult(string.Empty, false, true);
        }
    }
}
