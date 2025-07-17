using System.Security.Cryptography;
using System.Text;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumberChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        public string GeneratePassword(int minLength = 14, int maxLength = 20)
        {
            if (minLength < 4)
                throw new ArgumentException("Minimum length must be at least 4 to ensure all character types can be included");
            
            if (maxLength < minLength)
                throw new ArgumentException("Maximum length must be greater than or equal to minimum length");

            using var rng = RandomNumberGenerator.Create();
            
            // Determine random length between min and max
            var length = GetRandomNumber(rng, minLength, maxLength + 1);
            
            var password = new StringBuilder(length);
            var allChars = LowercaseChars + UppercaseChars + NumberChars + SpecialChars;
            
            // Ensure we have at least one character from each required type
            password.Append(GetRandomCharacter(rng, LowercaseChars)); // At least one lowercase
            password.Append(GetRandomCharacter(rng, UppercaseChars)); // At least one uppercase
            password.Append(GetRandomCharacter(rng, NumberChars));    // At least one number
            
            // Fill the rest with random characters from all sets
            for (int i = 3; i < length; i++)
            {
                password.Append(GetRandomCharacter(rng, allChars));
            }
            
            // Shuffle the password to avoid predictable patterns
            return ShuffleString(rng, password.ToString());
        }

        private static char GetRandomCharacter(RandomNumberGenerator rng, string characterSet)
        {
            var randomIndex = GetRandomNumber(rng, 0, characterSet.Length);
            return characterSet[randomIndex];
        }

        private static int GetRandomNumber(RandomNumberGenerator rng, int minValue, int maxValue)
        {
            var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            var randomValue = BitConverter.ToUInt32(randomBytes, 0);
            return (int)(randomValue % (maxValue - minValue)) + minValue;
        }

        private static string ShuffleString(RandomNumberGenerator rng, string input)
        {
            var array = input.ToCharArray();
            
            // Fisher-Yates shuffle algorithm
            for (int i = array.Length - 1; i > 0; i--)
            {
                var j = GetRandomNumber(rng, 0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            
            return new string(array);
        }
    }
} 