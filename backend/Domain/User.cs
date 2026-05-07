using System;
using ToolLendingPlatform.Domain.Exceptions;

namespace ToolLendingPlatform.Domain
{
    /// <summary>
    /// User aggregate root.
    /// Represents a person using the Tool Lending Platform.
    /// Immutable after creation; all state is set in constructor.
    /// </summary>
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Constructor for new registrations.
        /// </summary>
        public User(string username, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidUsernameException("Username cannot be empty");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new InvalidPasswordException("Password hash cannot be empty");

            Username = username;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// EF Core requires a parameterless constructor for entity loading from database.
        /// </summary>
        protected User() { }
    }
}
