using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    public interface IFederationCryptographyService
    {
        Task<KeyPairResult> GenerateRSAKeyPairAsync();
        Task<string> GenerateSecureTokenAsync(int lengthBytes = 32);
        Task<string> SignDataAsync(string data, string privateKeyBase64);
        Task<bool> VerifySignatureAsync(string data, string signatureBase64, string publicKeyBase64);
        Task<string> EncryptAsync(string plaintext, string publicKeyBase64);
        Task<string> DecryptAsync(string ciphertext, string privateKeyBase64);
        Task<SymmetricEncryptionResult> EncryptSymmetricAsync(string plaintext, string keyBase64);
        Task<string> DecryptSymmetricAsync(string ciphertext, string keyBase64, string ivBase64);
        Task<string> GenerateSymmetricKeyAsync();
        Task<string> DeriveKeyFromPasswordAsync(string password, string saltBase64);
        Task<string> GenerateSaltAsync();
        Task<string> HashDataAsync(string data);
        Task<HandshakeChallengeResult> GenerateHandshakeChallengeAsync();
        Task<bool> ValidateHandshakeResponseAsync(string challenge, string response, string publicKeyBase64);
        Task<string> CreateSecureMessageAsync(object data, string privateKeyBase64);
        Task<T?> VerifyAndDecodeSecureMessageAsync<T>(string secureMessage, string publicKeyBase64);
    }
} 