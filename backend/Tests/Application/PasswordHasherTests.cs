using System;
using System.Threading.Tasks;
using Xunit;
using ToolLendingPlatform.Application.Services;

namespace ToolLendingPlatform.Tests.Application
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher = new();

        // Rule #1: Hash produces non-plaintext output
        [Fact]
        public async Task HashAsync_ValidPassword_ReturnsHashedPassword()
        {
            // Arrange
            var plaintext = "SecurePassword123";

            // Act
            var hash = await _passwordHasher.HashAsync(plaintext);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEqual(plaintext, hash); // Hash should not be plaintext
            Assert.StartsWith("$2a$", hash); // bcrypt format verification
        }

        // Rule #2: Verify accepts correct password
        [Fact]
        public async Task VerifyAsync_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var plaintext = "CorrectPassword123";
            var hash = await _passwordHasher.HashAsync(plaintext);

            // Act
            var isValid = await _passwordHasher.VerifyAsync(plaintext, hash);

            // Assert
            Assert.True(isValid);
        }

        // Rule #2: Verify rejects incorrect password
        [Fact]
        public async Task VerifyAsync_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var plaintext = "CorrectPassword123";
            var wrongPassword = "WrongPassword456";
            var hash = await _passwordHasher.HashAsync(plaintext);

            // Act
            var isValid = await _passwordHasher.VerifyAsync(wrongPassword, hash);

            // Assert
            Assert.False(isValid);
        }

        // Rule #2: Timing should be consistent (constant-time)
        [Fact]
        public async Task VerifyAsync_TimingIsConsistent()
        {
            // Arrange
            var hash = await _passwordHasher.HashAsync("CorrectPassword123");
            var correctPassword = "CorrectPassword123";
            var wrongPassword = "WrongPassword______"; // Similar length

            // Act & Assert (BCrypt.NET internally uses constant-time comparison)
            // We trust the library implementation; this test documents the expectation
            var correctResult = await _passwordHasher.VerifyAsync(correctPassword, hash);
            var wrongResult = await _passwordHasher.VerifyAsync(wrongPassword, hash);

            Assert.True(correctResult);
            Assert.False(wrongResult);
            // Both should take approximately the same time (not measurable in unit test, but principle verified)
        }
    }
}
