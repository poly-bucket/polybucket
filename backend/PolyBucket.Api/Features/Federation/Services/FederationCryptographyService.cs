using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Federation.Domain;

namespace PolyBucket.Api.Features.Federation.Services
{
    public class FederationCryptographyService(ILogger<FederationCryptographyService> logger) : IFederationCryptographyService
    {
        private readonly ILogger<FederationCryptographyService> _logger = logger;
        private const int KeySizeBytes = 32; // 256 bits for AES
        private const int IvSizeBytes = 16; // 128 bits for AES IV
        private const int RSAKeySize = 2048; // RSA key size in bits
        private const int SaltSizeBytes = 32; // 256 bits for salt
        private const int IterationCount = 100000; // PBKDF2 iterations

        public Task<KeyPairResult> GenerateRSAKeyPairAsync()
        {
            try
            {
                using var rsa = RSA.Create(RSAKeySize);
                var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

                return Task.FromResult(new KeyPairResult
                {
                    PrivateKey = privateKey,
                    PublicKey = publicKey
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate RSA key pair");
                throw new CryptographyException("Failed to generate RSA key pair", ex);
            }
        }

        public Task<string> GenerateSecureTokenAsync(int lengthBytes = KeySizeBytes)
        {
            try
            {
                using var rng = RandomNumberGenerator.Create();
                var bytes = new byte[lengthBytes];
                rng.GetBytes(bytes);
                return Task.FromResult(Convert.ToBase64String(bytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate secure token");
                throw new CryptographyException("Failed to generate secure token", ex);
            }
        }

        public Task<string> SignDataAsync(string data, string privateKeyBase64)
        {
            try
            {
                var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                
                return Task.FromResult(Convert.ToBase64String(signatureBytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sign data");
                throw new CryptographyException("Failed to sign data", ex);
            }
        }

        public Task<bool> VerifySignatureAsync(string data, string signatureBase64, string publicKeyBase64)
        {
            try
            {
                var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
                var signatureBytes = Convert.FromBase64String(signatureBase64);
                
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var isValid = rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                
                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify signature");
                return Task.FromResult(false);
            }
        }

        public Task<string> EncryptAsync(string plaintext, string publicKeyBase64)
        {
            try
            {
                var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                var encryptedBytes = rsa.Encrypt(plaintextBytes, RSAEncryptionPadding.OaepSHA256);
                
                return Task.FromResult(Convert.ToBase64String(encryptedBytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt data");
                throw new CryptographyException("Failed to encrypt data", ex);
            }
        }

        public Task<string> DecryptAsync(string ciphertext, string privateKeyBase64)
        {
            try
            {
                var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
                var ciphertextBytes = Convert.FromBase64String(ciphertext);
                
                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                
                var decryptedBytes = rsa.Decrypt(ciphertextBytes, RSAEncryptionPadding.OaepSHA256);
                
                return Task.FromResult(Encoding.UTF8.GetString(decryptedBytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt data");
                throw new CryptographyException("Failed to decrypt data", ex);
            }
        }

        public Task<SymmetricEncryptionResult> EncryptSymmetricAsync(string plaintext, string keyBase64)
        {
            try
            {
                var key = Convert.FromBase64String(keyBase64);
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();
                
                using var encryptor = aes.CreateEncryptor();
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                var encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                
                return Task.FromResult(new SymmetricEncryptionResult
                {
                    CipherText = Convert.ToBase64String(encryptedBytes),
                    IV = Convert.ToBase64String(aes.IV)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt data with symmetric key");
                throw new CryptographyException("Failed to encrypt data with symmetric key", ex);
            }
        }

        public Task<string> DecryptSymmetricAsync(string ciphertext, string keyBase64, string ivBase64)
        {
            try
            {
                var key = Convert.FromBase64String(keyBase64);
                var iv = Convert.FromBase64String(ivBase64);
                var ciphertextBytes = Convert.FromBase64String(ciphertext);
                
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                
                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
                
                return Task.FromResult(Encoding.UTF8.GetString(decryptedBytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt data with symmetric key");
                throw new CryptographyException("Failed to decrypt data with symmetric key", ex);
            }
        }

        public Task<string> GenerateSymmetricKeyAsync()
        {
            try
            {
                using var aes = Aes.Create();
                aes.GenerateKey();
                return Task.FromResult(Convert.ToBase64String(aes.Key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate symmetric key");
                throw new CryptographyException("Failed to generate symmetric key", ex);
            }
        }

        public Task<string> DeriveKeyFromPasswordAsync(string password, string saltBase64)
        {
            try
            {
                var salt = Convert.FromBase64String(saltBase64);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA256);
                var key = pbkdf2.GetBytes(KeySizeBytes);
                return Task.FromResult(Convert.ToBase64String(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to derive key from password");
                throw new CryptographyException("Failed to derive key from password", ex);
            }
        }

        public Task<string> GenerateSaltAsync()
        {
            try
            {
                using var rng = RandomNumberGenerator.Create();
                var salt = new byte[SaltSizeBytes];
                rng.GetBytes(salt);
                return Task.FromResult(Convert.ToBase64String(salt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate salt");
                throw new CryptographyException("Failed to generate salt", ex);
            }
        }

        public Task<string> HashDataAsync(string data)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hashBytes = sha256.ComputeHash(dataBytes);
                return Task.FromResult(Convert.ToBase64String(hashBytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to hash data");
                throw new CryptographyException("Failed to hash data", ex);
            }
        }

        public Task<HandshakeChallengeResult> GenerateHandshakeChallengeAsync()
        {
            try
            {
                var challenge = Guid.NewGuid().ToString("N");
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                
                var challengeData = new
                {
                    challenge,
                    timestamp,
                    nonce,
                    expires_at = timestamp + 300 // 5 minutes
                };
                
                var challengeJson = JsonSerializer.Serialize(challengeData);
                var challengeHash = HashDataAsync(challengeJson).Result;
                
                return Task.FromResult(new HandshakeChallengeResult
                {
                    Challenge = challengeJson,
                    ChallengeHash = challengeHash,
                    ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(challengeData.expires_at).DateTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate handshake challenge");
                throw new CryptographyException("Failed to generate handshake challenge", ex);
            }
        }

        public Task<bool> ValidateHandshakeResponseAsync(string challenge, string response, string publicKeyBase64)
        {
            try
            {
                // Parse challenge to check expiration
                var challengeData = JsonSerializer.Deserialize<JsonElement>(challenge);
                var expiresAt = challengeData.GetProperty("expires_at").GetInt64();
                
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresAt)
                {
                    _logger.LogWarning("Handshake challenge has expired");
                    return Task.FromResult(false);
                }
                
                // Verify the response signature
                return VerifySignatureAsync(challenge, response, publicKeyBase64);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate handshake response");
                return Task.FromResult(false);
            }
        }

        public Task<string> CreateSecureMessageAsync(object data, string privateKeyBase64)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var messageId = Guid.NewGuid().ToString("N");
                
                var message = new
                {
                    id = messageId,
                    timestamp,
                    data = json
                };
                
                var messageJson = JsonSerializer.Serialize(message);
                var signature = SignDataAsync(messageJson, privateKeyBase64).Result;
                
                var secureMessage = new
                {
                    message = messageJson,
                    signature
                };
                
                return Task.FromResult(JsonSerializer.Serialize(secureMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create secure message");
                throw new CryptographyException("Failed to create secure message", ex);
            }
        }

        public Task<T?> VerifyAndDecodeSecureMessageAsync<T>(string secureMessage, string publicKeyBase64)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<JsonElement>(secureMessage);
                var message = envelope.GetProperty("message").GetString()!;
                var signature = envelope.GetProperty("signature").GetString()!;
                
                var isValid = VerifySignatureAsync(message, signature, publicKeyBase64).Result;
                if (!isValid)
                {
                    _logger.LogWarning("Invalid signature in secure message");
                    return Task.FromResult<T?>(default);
                }
                
                var messageData = JsonSerializer.Deserialize<JsonElement>(message);
                var dataJson = messageData.GetProperty("data").GetString()!;
                var data = JsonSerializer.Deserialize<T>(dataJson);
                
                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify and decode secure message");
                return Task.FromResult<T?>(default);
            }
        }
    }

    public class KeyPairResult
    {
        public string PrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
    }

    public class SymmetricEncryptionResult
    {
        public string CipherText { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
    }

    public class HandshakeChallengeResult
    {
        public string Challenge { get; set; } = string.Empty;
        public string ChallengeHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class CryptographyException : Exception
    {
        public CryptographyException(string message) : base(message) { }
        public CryptographyException(string message, Exception innerException) : base(message, innerException) { }
    }
} 