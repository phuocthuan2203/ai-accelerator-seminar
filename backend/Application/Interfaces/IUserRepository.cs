using System;
using System.Threading.Tasks;
using ToolLendingPlatform.Domain;

namespace ToolLendingPlatform.Application.Interfaces
{
    /// <summary>
    /// Repository interface for User aggregate persistence.
    /// Implements the Repository pattern to abstract data access.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Query user by username; return User or null if not found.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Persist a User entity to the database; return the saved user with ID.
        /// </summary>
        Task<User> SaveAsync(User user);

        /// <summary>
        /// Check if username exists without loading full User object.
        /// </summary>
        Task<bool> ExistsAsync(string username);
    }
}
