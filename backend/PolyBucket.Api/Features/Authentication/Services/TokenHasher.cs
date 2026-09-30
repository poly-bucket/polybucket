using System;
using System.Security.Cryptography;
using System.Text;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public static class TokenHasher
    {
        public static string Hash(string rawToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(rawToken);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
        }
    }
}
