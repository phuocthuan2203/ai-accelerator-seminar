using System;

namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when attempting to register with a username that already exists.
    /// </summary>
    public class DuplicateUsernameException : Exception
    {
        public DuplicateUsernameException(string username)
            : base($"Username '{username}' is already taken") { }
    }
}
