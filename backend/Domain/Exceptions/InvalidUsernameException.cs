using System;

namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when username validation fails (e.g., null, empty, invalid format).
    /// </summary>
    public class InvalidUsernameException : Exception
    {
        public InvalidUsernameException(string message = "Invalid username")
            : base(message) { }
    }
}
