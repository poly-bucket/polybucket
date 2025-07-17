namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IPasswordGenerator
    {
        /// <summary>
        /// Generates a secure password with specified length constraints and character requirements
        /// </summary>
        /// <param name="minLength">Minimum password length (default: 14)</param>
        /// <param name="maxLength">Maximum password length (default: 20)</param>
        /// <returns>A randomly generated password meeting the specified criteria</returns>
        string GeneratePassword(int minLength = 14, int maxLength = 20);
    }
} 