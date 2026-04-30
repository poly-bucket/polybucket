using System.Collections.Generic;
using System.Security.Cryptography;
using PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Tests;

internal static class TotpTestHelper
{
    private static readonly TwoFactorAuthSettings Settings = new();

    public static string GenerateCurrentTotp(string secretBase32)
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var step = unixTime / Settings.TokenExpirySeconds;
        return GenerateTotp(secretBase32, step);
    }

    private static string GenerateTotp(string secretKey, long timeStep)
    {
        var keyBytes = ConvertFromBase32(secretKey);
        var timeStepBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            System.Array.Reverse(timeStepBytes);
        }

        using var hmac = new HMACSHA1(keyBytes);
        var hash = hmac.ComputeHash(timeStepBytes);
        var offset = hash[hash.Length - 1] & 0xf;
        var code = ((hash[offset] & 0x7f) << 24) |
                   ((hash[offset + 1] & 0xff) << 16) |
                   ((hash[offset + 2] & 0xff) << 8) |
                   (hash[offset + 3] & 0xff);
        return (code % 1000000).ToString("D6");
    }

    private static byte[] ConvertFromBase32(string input)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new List<byte>();
        var bits = 0;
        var value = 0;

        foreach (var c in input.ToUpperInvariant())
        {
            var index = base32Chars.IndexOf(c);
            if (index == -1) continue;

            value = (value << 5) | index;
            bits += 5;

            while (bits >= 8)
            {
                result.Add((byte)(value >> (bits - 8)));
                bits -= 8;
            }
        }

        return result.ToArray();
    }
}
