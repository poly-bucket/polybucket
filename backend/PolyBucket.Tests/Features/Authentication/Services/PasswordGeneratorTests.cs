using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Services
{
    public class PasswordGeneratorTests
    {
        private readonly PasswordGenerator _passwordGenerator;

        public PasswordGeneratorTests()
        {
            _passwordGenerator = new PasswordGenerator();
        }

        [Fact]
        public void GeneratePassword_ShouldGeneratePasswordWithinLengthRange()
        {
            // Arrange & Act
            var password = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.InRange(password.Length, 14, 20);
        }

        [Fact]
        public void GeneratePassword_ShouldContainAtLeastOneAlphaCharacter()
        {
            // Arrange & Act
            var password = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.True(password.Any(char.IsLetter), "Password should contain at least one alphabetic character");
        }

        [Fact]
        public void GeneratePassword_ShouldContainAtLeastOneNumber()
        {
            // Arrange & Act
            var password = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.True(password.Any(char.IsDigit), "Password should contain at least one numeric character");
        }

        [Fact]
        public void GeneratePassword_ShouldContainAtLeastOneLowercaseLetter()
        {
            // Arrange & Act
            var password = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.True(password.Any(char.IsLower), "Password should contain at least one lowercase letter");
        }

        [Fact]
        public void GeneratePassword_ShouldContainAtLeastOneUppercaseLetter()
        {
            // Arrange & Act
            var password = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.True(password.Any(char.IsUpper), "Password should contain at least one uppercase letter");
        }

        [Theory]
        [InlineData(14, 16)]
        [InlineData(16, 18)]
        [InlineData(18, 20)]
        public void GeneratePassword_WithCustomLengthRange_ShouldRespectLimits(int minLength, int maxLength)
        {
            // Arrange & Act
            var password = _passwordGenerator.GeneratePassword(minLength, maxLength);

            // Assert
            Assert.InRange(password.Length, minLength, maxLength);
            Assert.True(password.Any(char.IsLetter), "Password should contain at least one alphabetic character");
            Assert.True(password.Any(char.IsDigit), "Password should contain at least one numeric character");
        }

        [Fact]
        public void GeneratePassword_GenerateMultiple_ShouldProduceDifferentPasswords()
        {
            // Arrange & Act
            var password1 = _passwordGenerator.GeneratePassword();
            var password2 = _passwordGenerator.GeneratePassword();
            var password3 = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.NotEqual(password1, password2);
            Assert.NotEqual(password1, password3);
            Assert.NotEqual(password2, password3);
        }

        [Fact]
        public void GeneratePassword_ShouldOnlyContainValidCharacters()
        {
            // Arrange
            var validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
            
            // Act
            var password = _passwordGenerator.GeneratePassword();

            // Assert
            Assert.True(password.All(c => validCharacters.Contains(c)), 
                $"Password contains invalid characters. Password: {password}");
        }

        [Theory]
        [InlineData(3)]
        [InlineData(2)]
        [InlineData(1)]
        public void GeneratePassword_WithTooShortMinLength_ShouldThrowArgumentException(int minLength)
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordGenerator.GeneratePassword(minLength, 20));
        }

        [Fact]
        public void GeneratePassword_WithMaxLengthLessThanMinLength_ShouldThrowArgumentException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordGenerator.GeneratePassword(15, 10));
        }

        [Fact]
        public void GeneratePassword_RepeatedCalls_ShouldBeSufficientlyRandom()
        {
            // Arrange
            var passwords = new string[100];
            
            // Act
            for (int i = 0; i < passwords.Length; i++)
            {
                passwords[i] = _passwordGenerator.GeneratePassword();
            }

            // Assert
            var uniquePasswords = passwords.Distinct().Count();
            Assert.True(uniquePasswords >= 95, $"Expected at least 95 unique passwords out of 100, got {uniquePasswords}");
        }
    }
} 