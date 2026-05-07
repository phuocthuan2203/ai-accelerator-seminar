using System;

namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when login fails (user not found or password incorrect).
    /// Generic message used to prevent username enumeration.
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message = "Invalid username or password")
            : base(message) { }
    }
}
