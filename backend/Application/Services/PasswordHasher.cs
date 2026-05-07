using System;
using System.Threading.Tasks;
using BCrypt.Net;
using ToolLendingPlatform.Application.Interfaces;

namespace ToolLendingPlatform.Application.Services
{
    /// <summary>
    /// PasswordHasher using BCrypt.Net library.
    /// Rule #1: Hashes passwords with bcrypt algorithm (salt rounds ≥ 10).
    /// Rule #2: Verifies passwords using BCrypt.Net which implements constant-time comparison.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltRounds = 10; // bcrypt salt rounds (≥ 10 per security best practices)

        /// <summary>
        /// Rule #1: Hash plaintext password using bcrypt.
        /// Salt rounds = 10 ensures reasonable performance + security balance.
        /// </summary>
        public Task<string> HashAsync(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                throw new ArgumentException("Password cannot be null or empty", nameof(plaintext));

            var hash = BCrypt.Net.BCrypt.HashPassword(plaintext, SaltRounds);
            return Task.FromResult(hash);
        }

        /// <summary>
        /// Rule #2: Verify plaintext against hash using constant-time comparison.
        /// BCrypt.Net.BCrypt.Verify() internally uses constant-time comparison to prevent timing attacks.
        /// </summary>
        public Task<bool> VerifyAsync(string plaintext, string hash)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(hash))
                return Task.FromResult(false);

            bool isValid = BCrypt.Net.BCrypt.Verify(plaintext, hash);
            return Task.FromResult(isValid);
        }
    }
}
