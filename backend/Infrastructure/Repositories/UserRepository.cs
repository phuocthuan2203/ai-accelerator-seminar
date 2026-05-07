using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Infrastructure.Data;

namespace ToolLendingPlatform.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IUserRepository.
    /// Uses EF LINQ (parameterized queries) to prevent SQL injection.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ToolLendingDbContext _dbContext;

        public UserRepository(ToolLendingDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Query user by username using parameterized EF LINQ.
        /// Returns null if not found (no exception thrown).
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            // EF Core translates LINQ to parameterized SQL automatically
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Persist User entity to database.
        /// If username already exists, EF Core will throw DbUpdateException (unique constraint violation).
        /// Returns the saved user (with ID populated by database).
        /// </summary>
        public async Task<User> SaveAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dbContext.Users.Add(user);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Unique constraint violation on username
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") ?? false)
                {
                    throw new InvalidOperationException($"Username '{user.Username}' already exists", ex);
                }
                throw;
            }

            return user;
        }

        /// <summary>
        /// Check if username exists without loading full User object.
        /// Lightweight query for registration validation.
        /// </summary>
        public async Task<bool> ExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Username == username);
        }
    }
}
