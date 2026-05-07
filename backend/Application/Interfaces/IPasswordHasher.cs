using System;
using System.Threading.Tasks;

namespace ToolLendingPlatform.Application.Interfaces
{
    /// <summary>
    /// Interface for password hashing and verification.
    /// Rule #1, #2: Contract for bcrypt-based password operations.
    /// </summary>
    public interface IPasswordHasher
    {
        /// Rule #1: Hash plaintext password using bcrypt
        Task<string> HashAsync(string plaintext);

        /// Rule #2: Verify plaintext against hash using constant-time comparison
        Task<bool> VerifyAsync(string plaintext, string hash);
    }
}
