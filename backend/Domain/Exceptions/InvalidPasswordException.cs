using System;

namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when password validation fails (e.g., null, empty, insufficient complexity).
    /// </summary>
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string message = "Invalid password")
            : base(message) { }
    }
}
